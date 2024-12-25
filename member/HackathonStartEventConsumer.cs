namespace member;

using domain;
using Model;
using MassTransit;

public class HackathonStartEventConsumer
    : IConsumer<HackathonStartEvent>
{
    private readonly Member _member;
    private readonly AllMembers _allMembers;
    private readonly IPublishEndpoint _publishEndpoint;

    public HackathonStartEventConsumer(Member member, AllMembers allMembers, IPublishEndpoint publishEndpoint)
    {
        _member = member;
        _allMembers = allMembers;
        _publishEndpoint = publishEndpoint;
    }


    public async Task Consume(ConsumeContext<HackathonStartEvent> context)
    {
        Console.WriteLine($"Хакатон {context.Message.HackathonId} начался!");
        Console.WriteLine($"Участник: {_member}");

        var state = new WorkerState.MemberState(_member, _allMembers);
        var generator = new PreferencesGenerator(_member, _allMembers);
        var preferences = generator.GenerateRandomSortedPreferences();
        var preferencesResponse = new Preferences(context.Message.HackathonId,
            state.Member, preferences);

        await _publishEndpoint.Publish(preferencesResponse);

        Console.WriteLine(
            $"ID хакатона: {context.Message.HackathonId}. Сгенерированные предпочтения: {string.Join(", ", preferences)}.");
    }
}