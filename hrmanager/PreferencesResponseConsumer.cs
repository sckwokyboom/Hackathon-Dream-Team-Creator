namespace hrmanager;

using domain;
using System.Net;
using System.Text;
using System.Text.Json;
using MassTransit;

public class PreferencesResponseConsumer(
    DirectorSettings directorSettings,
    TeamCreationService teamCreationService) : IConsumer<Preferences>
{
    private readonly HttpClient _client = new();

    public async Task Consume(ConsumeContext<Preferences> context)
    {
        Console.WriteLine($"Хакатон ID: {context.Message.HackathonId}.\n" +
                          $"Участник, для которого получены предпочтения: {context.Message.Member}.");

        teamCreationService.AddPreferences(context.Message);

        if (teamCreationService.IsAllPreferencesForHackathonArePresented(context.Message.HackathonId))
        {
            Console.WriteLine("Все предпочтения получены, формирование команд...");
            var teams = teamCreationService.BuildTeams(context.Message.HackathonId);
            Console.WriteLine("Команды сформированы. Количество команд: " + teams.Count);
            await NotifyDirectorAsync(new CreatedTeams(context.Message.HackathonId, teams));
        }
    }


    private async Task NotifyDirectorAsync(CreatedTeams request)
    {
        Console.WriteLine("Уведомление HR-директора о созданных командах...");

        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8,
            "application/json");

        try
        {
            var response =
                await _client.PostAsync($"http://{directorSettings.Uri}/teams",
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