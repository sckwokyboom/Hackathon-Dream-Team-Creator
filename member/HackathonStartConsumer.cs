namespace member;

using domain;
using Model;
using MassTransit;

public class HackathonStartConsumer
    : IConsumer<HackathonStartEvent>
{
    private readonly Member member;
    private readonly AllMembers _allMembers;
    private readonly IPublishEndpoint publishEndpoint;

    public HackathonStartConsumer(Member member, AllMembers allMembers, IPublishEndpoint publishEndpoint)
    {
        this.member = member;
        _allMembers = allMembers;
        this.publishEndpoint = publishEndpoint;
    }


    public Task Consume(ConsumeContext<HackathonStartEvent> context)
    {
        Console.WriteLine($"Хакатон {context.Message.HackathonId} начался!");
        Console.WriteLine($"Участник: {member}");

        var state = new WorkerState.MemberState(member, _allMembers);
        var generator = new PreferencesGenerator(member, _allMembers);
        var preferences = generator.GenerateRandomSortedPreferences();
        var preferencesResponse = new Preferences(context.Message.HackathonId,
            state.Member, preferences);

        publishEndpoint.Publish(preferencesResponse);

        Console.WriteLine(
            $"ID хакатона: {context.Message.HackathonId}. Сгенерированные предпочтения: {string.Join(", ", preferences)}.");
        return Task.CompletedTask;
    }
}