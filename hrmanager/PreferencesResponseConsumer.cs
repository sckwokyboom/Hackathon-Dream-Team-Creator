namespace hrmanager;

using domain;
using System.Net;
using System.Text;
using System.Text.Json;
using MassTransit;

public class PreferencesResponseConsumer(
    HrDirectorParameters hrDirectorParameters,
    DreamTeamFormationService dreamTeamFormationService) : IConsumer<Preferences>
{
    private readonly HttpClient _client = new();

    public async Task Consume(ConsumeContext<Preferences> context)
    {
        Console.WriteLine($"Хакатон ID: {context.Message.HackathonId}.\n" +
                          $"Участник, для которого получены предпочтения: {context.Message.Member}.");

        dreamTeamFormationService.AddPreferences(context.Message);

        if (dreamTeamFormationService.IsAllPreferencesForHackathonArePresented(context.Message.HackathonId))
        {
            Console.WriteLine("Все предпочтения получены, формирование команд...");
            var teams = dreamTeamFormationService.BuildTeams(context.Message.HackathonId);
            Console.WriteLine("Команды сформированы. Количество команд: " + teams.Count);
            await NotifyDirectorAsync(new CreatedTeams(context.Message.HackathonId, teams));
        }
    }


    private async Task NotifyDirectorAsync(CreatedTeams request)
    {
        Console.WriteLine("Уведомление HR-директора о созданных командах...");

        var payload = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        while (true)
        {
            try

            {
                var response =
                    await _client.PostAsync($"http://{hrDirectorParameters.Uri}/teams",
                        payload);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Уведомление HR-директора успешно отправлено.");
                    break;
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
}