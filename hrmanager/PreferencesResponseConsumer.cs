namespace hrmanager;

using System.Collections.Concurrent;
using domain;
using System.Net;
using System.Text;
using System.Text.Json;
using MassTransit;

public class PreferencesResponseConsumer(
    HrDirectorParameters hrDirectorParameters,
    DreamTeamFormationService dreamTeamFormationService) : IConsumer<Preferences>
{
    private readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    private readonly ConcurrentDictionary<int, bool> _notifiedHackathons = new();
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _hackathonLocks = new();

    public async Task Consume(ConsumeContext<Preferences> context)
    {
        Console.WriteLine($"Хакатон ID: {context.Message.HackathonId}.\n" +
                          $"Участник, для которого получены предпочтения: {context.Message.Member}.");

        dreamTeamFormationService.AddPreferences(context.Message);
        var hackathonLock = _hackathonLocks.GetOrAdd(context.Message.HackathonId, _ => new SemaphoreSlim(1, 1));
        await hackathonLock.WaitAsync();
        try
        {
            if (dreamTeamFormationService.IsAllPreferencesForHackathonArePresented(context.Message.HackathonId))
            {
                if (_notifiedHackathons.TryAdd(context.Message.HackathonId, true))
                {
                    Console.WriteLine("Все предпочтения получены, формирование команд...");
                    var teams = dreamTeamFormationService.BuildTeams(context.Message.HackathonId);
                    Console.WriteLine("Команды сформированы. Количество команд: " + teams.Count);
                    await NotifyDirectorAsync(new CreatedTeams(context.Message.HackathonId, teams));
                }
                else
                {
                    Console.WriteLine("Уведомление для этого хакатона уже было отправлено.");
                }
            }
        }
        finally
        {
            Console.WriteLine($"Все предпочтения для хакатона {context.Message.HackathonId} обработаны.");
            hackathonLock.Release();
            _hackathonLocks.TryRemove(context.Message.HackathonId, out _); // Удаляем мьютекс после использования
        }
    }


    private async Task NotifyDirectorAsync(CreatedTeams request)
    {
        Console.WriteLine("Уведомление HR-директора о созданных командах...");

        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        try
        {
            var response =
                await _client.PostAsync($"http://{hrDirectorParameters.Uri}/teams",
                    payload);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("Уведомление HR-директора успешно отправлено.");
            }
            else
            {
                Console.WriteLine($"Уведомление не отправлено, код ошибки : {response.StatusCode}.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при отправке уведомления HR-директору о созданных командах.");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}