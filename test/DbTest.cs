namespace test;

using domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using hrdirector;
using hrdirector.Entities;
using MassTransit;
using Microsoft.Data.Sqlite;
using Moq;

public class TestDatabaseServerFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public IDbContextFactory<HackathonDbContext> DbContextFactory { get; }

    public TestDatabaseServerFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.OpenAsync();

        var serviceProvider = new ServiceCollection()
            .AddDbContextFactory<HackathonDbContext>(options =>
                options.UseSqlite(_connection))
            .BuildServiceProvider();

        DbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<HackathonDbContext>>();

        using var context = DbContextFactory.CreateDbContext();
        context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Close();
    }
}

public class MetricsCalculationServiceTests(TestDatabaseServerFixture fixture)
    : IClassFixture<TestDatabaseServerFixture>
{
    private readonly IDbContextFactory<HackathonDbContext> _dbContextFactory = fixture.DbContextFactory;

    [Fact(DisplayName = "Расчёт и запись среднего гармонического")]
    public async void TestCalculateAndSetMetricsToDb()
    {
        using var context = _dbContextFactory.CreateDbContext();
        const int hackathonId = 1337;
        var team = new Team(new Member(1, EmployeeType.Junior), new Member(2, EmployeeType.TeamLead));
        var preferences1 = new Preferences(hackathonId, team.Junior, [team.TeamLead.Id]);
        var preferences2 = new Preferences(hackathonId, team.TeamLead, [team.Junior.Id]);
        var hackathon = new Hackathon()
        {
            HackathonId = hackathonId,
            Teams = [team],
            Preferences = [preferences1, preferences2]
        };
        context.Entities.Add(hackathon);
        await context.SaveChangesAsync();
        var service = new MetricCalculationService(_dbContextFactory, Mock.Of<IPublishEndpoint>());

        await service.SetHarmonyAsync(hackathonId);

        var harmony = service.GetHackathonById(hackathonId)!.Harmony;
        Assert.Equal(1, harmony);
    }

    [Fact(DisplayName = "Запись информации о мероприятии в БД.")]
    public async Task TestAddHackathonAndPreferencesToDb()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();
        var service = new MetricCalculationService(_dbContextFactory, Mock.Of<IPublishEndpoint>());
        await service.StartHackathonsAsync(1);
        var hackathonId = context.Entities.First().HackathonId;
        var team = new Team(new Member(1, EmployeeType.Junior), new Member(1, EmployeeType.TeamLead));
        var preferences1 = new Preferences(hackathonId, new Member(1, EmployeeType.Junior), new List<int> { 1 });
        var preferences2 = new Preferences(hackathonId, new Member(1, EmployeeType.TeamLead), new List<int> { 1 });
        await service.AddPreferencesFromHackathonToDbAsync(hackathonId, preferences1);
        await service.AddPreferencesFromHackathonToDbAsync(hackathonId, preferences2);
        await service.AddTeamsFromHackathonToDbAsync(hackathonId, [team]);

        var hackathon = context.Entities.FirstOrDefault(e => e.HackathonId == hackathonId);

        Assert.NotNull(hackathon);
        Assert.Equal(hackathonId, hackathon.HackathonId);
        Assert.Contains([team], t => t.Junior.Id == 1);
        Assert.Contains(hackathon.Preferences!, p => p.Member.Id == 1);
    }

    [Fact(DisplayName = "Чтение информации о мероприятии из БД")]
    public void TestReadHackathonFromDbById()
    {
        using var context = _dbContextFactory.CreateDbContext();
        const int hackathonId = 13371;
        var hackathon = new Hackathon { HackathonId = hackathonId };
        context.Entities.Add(hackathon);
        context.SaveChanges();
        var service = new MetricCalculationService(_dbContextFactory, Mock.Of<IPublishEndpoint>());

        var retrievedHackathon = service.GetHackathonById(hackathonId);

        Assert.NotNull(retrievedHackathon);
        Assert.Equal(hackathonId, retrievedHackathon!.HackathonId);
    }
}