using System.Collections.Concurrent;
using MassTransit.Internals;

namespace hrdirector;

using domain;
using System.Data.Entity.Core;
using Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

public class MetricCalculationService(
    IDbContextFactory<HackathonDbContext> contextFactory,
    IPublishEndpoint publishEndpoint)
{
    private readonly ConcurrentDictionary<int, int> _pendingPreferences = new();
    private readonly Dictionary<int, TaskCompletionSource<bool>> _preferencesCompletion = new();
    private const int TotalPreferencesCount = 10;
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _hackathonLocks = new();

    public async Task AddPreferencesFromHackathonToDbAsync(int hackathonId, Preferences preferences)
    {
        _pendingPreferences.AddOrUpdate(hackathonId, 1, (_, count) => count + 1);
        var semaphore = _hackathonLocks.GetOrAdd(hackathonId, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);
            if (hackathon == null)
            {
                throw new InvalidOperationException("Не существует хакатона с ID: " + hackathonId);
            }

            Console.WriteLine($"Сохранение в базу данных предпочтений участников хакатона: {hackathonId}.");
            if (hackathon.Preferences != null && !hackathon.Preferences.Contains(preferences))
            {
                hackathon.Preferences!.Add(preferences);
            }
            else
            {
                await Console.Error.WriteLineAsync(
                    $"Произошла ошибка при загрузке хакатона. Предпочтения уже добавлены или хакатон равен null.");
            }

            await context.Entities.AddAsync(hackathon);
            context.Entities.Update(hackathon);
            await context.SaveChangesAsync();
            Console.WriteLine($"Добавлено {hackathon.Preferences!.Count} предпочтений.");
            if (!_preferencesCompletion.TryGetValue(hackathonId, out TaskCompletionSource<bool>? value))
            {
                value = new TaskCompletionSource<bool>();
                _preferencesCompletion[hackathonId] = value;
            }

            if (hackathon.Preferences is { Count: >= TotalPreferencesCount })
            {
                Console.WriteLine($"Все предпочтения для хакатона {hackathonId} добавлены.");
                value.TrySetResult(true);
                _preferencesCompletion.Remove(hackathonId, out _);
            }
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(
                $"Ошибка при обновление предпочтений участников хакатона (id = {hackathonId}): {ex.Message}");
        }
        finally
        {
            semaphore.Release();
            if (_pendingPreferences[hackathonId] == 0)
            {
                _hackathonLocks.TryRemove(hackathonId, out _);
            }
        }
    }

    public async Task AddTeamsFromHackathonToDbAsync(int hackathonId, List<Team> teams)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        try
        {
            var completionTask = _preferencesCompletion.GetOrAdd(
                hackathonId,
                _ => new TaskCompletionSource<bool>()
            ).Task;

            await completionTask;

            var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);
            if (hackathon == null)
            {
                await Console.Error.WriteLineAsync("Хакатон является null.");
                throw new InvalidOperationException("Хакатон является null.");
            }

            if (hackathon.Preferences == null)
            {
                await Console.Error.WriteLineAsync("Preferences являются null.");
                throw new InvalidOperationException("Preferences являются null.");
            }

            if (hackathon.Teams == null)
            {
                Console.WriteLine("Команды на хакатоне равны null.");
                hackathon.Teams = [];
            }

            Console.WriteLine("Команды, которые уже были в хакатоне: " + string.Join(" ", hackathon.Teams));
            Console.WriteLine("Новые команды: " + string.Join(" ", teams));
            Console.WriteLine("Предпочтения: " + string.Join(" ", hackathon.Preferences));
            hackathon.Teams.AddRange(teams);
            hackathon.Harmony = CalculateHarmony(hackathon.Teams, hackathon.Preferences);
            Console.WriteLine($"Среднее по гармонии: {hackathon.Harmony}");
            context.Entities.Update(hackathon);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync("Ошибка при обновлении команды для хакатона (id= " + hackathonId +
                                               "): " +
                                               ex.Message);
        }
    }

    public static decimal CalculateHarmony(List<Team> teams, List<Preferences> preferences)
    {
        var max = teams.Count;

        var many = teams.SelectMany<Team, int>(team =>
            {
                var junPrefs = preferences.Find(it =>
                    it.Member.EmployeeType == EmployeeType.Junior && it.Member.Id == team.Junior.Id)!;
                var tlPrefs = preferences.Find(it =>
                    it.Member.EmployeeType == EmployeeType.TeamLead && it.Member.Id == team.TeamLead.Id)!;

                var junToTeamLeadRate = max - junPrefs.PreferencesList.IndexOf(team.TeamLead.Id);
                var teamLeadToJunRate = max - tlPrefs.PreferencesList.IndexOf(team.Junior.Id);

                return [junToTeamLeadRate, teamLeadToJunRate];
            }
        ).ToList();

        return HarmonyImpl(many);
    }

    public static decimal HarmonyImpl(List<int> seq)
    {
        var acc = seq.Sum(x => 1m / x);
        return seq.Count / acc;
    }

    public async Task StartHackathonsAsync(int totalHackathons)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        for (var i = 0; i < totalHackathons; i++)
        {
            // await Task.Delay(3000);
            var hackathonId = new Random().Next();
            var hackathon = new Hackathon { HackathonId = hackathonId };
            await context.Entities.AddAsync(hackathon);

            Console.WriteLine($"Хакатон №{i} начался.");
            var startEvent = new HackathonStartEvent(hackathonId);

            await context.SaveChangesAsync();

            await publishEndpoint.Publish(startEvent);
        }
    }

    public IEnumerable<int> GetAllHackathonIds()
    {
        using var context = contextFactory.CreateDbContext();
        return context.Entities.AsNoTracking().Select(e => e.HackathonId).ToList();
    }

    public async Task<Hackathon?> GetHackathonByIdAsync(int hackathonId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);
    }

    public async Task<decimal?> GetAverageHarmonyAsync()
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var harmonies = await context.Entities.Select(e => e.Harmony).ToListAsync();
        return harmonies.Count != 0 ? harmonies.Average() : null;
    }

    public async Task<List<Team>?> GetTeamsForHackathonAsync(int hackathonId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);

        return hackathon?.Teams;
    }

    public async Task<List<Preferences>?> GetPreferencesForHackathonAsync(int hackathonId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);
        return hackathon?.Preferences;
    }

    public async Task SetHarmonyAsync(int hackathonId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        try
        {
            var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);

            if (hackathon == null)
            {
                throw new EntityException("Hackathon not found");
            }

            hackathon.Harmony = CalculateHarmony(hackathon.Teams!, hackathon.Preferences!);
            context.Entities.Update(hackathon);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обновлении команды для хакатона (id = " + hackathonId + "): " + ex.Message);
        }
    }
}