﻿namespace hrdirector;

using domain;
using System.Data.Entity.Core;
using System.Runtime.CompilerServices;
using Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

public class MetricCalculationService(
    IDbContextFactory<ApplicationDbContext> contextFactory,
    IPublishEndpoint publishEndpoint)
{
    public async Task AddPreferencesFromHackathonToDbAsync(int hackathonId, Preferences preferences)
    {
        try
        {
            await using var context = await contextFactory.CreateDbContextAsync();
            var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);
            if (hackathon == null)
            {
                throw new InvalidOperationException("Не существует хакатона с ID: " + hackathonId);
            }

            Console.WriteLine($"Сохранение в базу данных предпочтений участников хакатона: {hackathonId}.");
            hackathon.Preferences!.Add(preferences);
            context.Entities.Update(hackathon);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(
                $"Ошибка при обновление предпочтений участников хакатона (id = {hackathonId}): {ex.Message}");
        }
    }

    public async Task AddTeamsFromHackathonToDbAsync(int hackathonId, List<Team> teams)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        try
        {
            var hackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);
            if (hackathon == null)
            {
                throw new InvalidOperationException("Хакатон является null.");
            }

            hackathon.Teams!.AddRange(teams);
            hackathon.Harmony = CalculateHarmony(hackathon.Teams, hackathon.Preferences!);
            context.Entities.Update(hackathon);
            Console.WriteLine("Harmony = " + hackathon.Harmony);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Ошибка при обновлении команды для хакатона (id= " + hackathonId + "): " +
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
            var hackathonId = new Random().Next();
            var hackathon = new Hackathon { HackathonId = hackathonId };
            context.Entities.Add(hackathon);

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

    public Hackathon? GetHackathonById(int hackathonId)
    {
        using var context = contextFactory.CreateDbContext();
        return context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);
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

            hackathon.Harmony = CalculateHarmony(hackathon.Teams, hackathon.Preferences);
            context.Entities.Update(hackathon);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при обновлении команды для хакатона (id = " + hackathonId + "): " + ex.Message);
        }
    }
}