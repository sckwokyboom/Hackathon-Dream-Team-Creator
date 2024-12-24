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
        _connection.Open();

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
        _connection.Dispose();
    }
}

public class MetricsCalculationServiceTests(TestDatabaseServerFixture fixture)
    : IClassFixture<TestDatabaseServerFixture>
{
    private readonly IDbContextFactory<HackathonDbContext> _dbContextFactory = fixture.DbContextFactory;

    [Fact(DisplayName = "Расчёт и запись среднего гармонического в базу данных.")]
    public async Task TestCalculateAndSetMetricsToDb()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        const int hackathonId = 1337;
        var team = new Team(new Member(1, EmployeeType.Junior), new Member(2, EmployeeType.TeamLead));
        var preferences1 = new Preferences(hackathonId, team.Junior, [team.TeamLead.Id]);
        var preferences2 = new Preferences(hackathonId, team.TeamLead, [team.Junior.Id]);
        var hackathon = new Hackathon
        {
            HackathonId = hackathonId,
            Teams = [team],
            Preferences = [preferences1, preferences2]
        };

        await context.Entities.AddAsync(hackathon);
        await context.SaveChangesAsync();

        var service = new MetricCalculationService(_dbContextFactory, Mock.Of<IPublishEndpoint>());
        await service.SetHarmonyAsync(hackathonId);
        await context.Entry(hackathon).ReloadAsync();

        var storedHackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);

        Assert.NotNull(storedHackathon);
        Assert.Equal(1, storedHackathon.Harmony);
    }

    [Fact(DisplayName = "Запись информации о мероприятии в БД.")]
    public async Task TestAddHackathonAndPreferencesToDb()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();

        var service = new MetricCalculationService(_dbContextFactory, Mock.Of<IPublishEndpoint>());
        await service.StartHackathonsAsync(1);
        
        var hackathon = await context.Entities.FirstAsync();
        var hackathonId = hackathon.HackathonId;
        var team = new Team(new Member(1, EmployeeType.Junior), new Member(1, EmployeeType.TeamLead));
        var preferences1 = new Preferences(hackathonId, new Member(1, EmployeeType.Junior), [1]);
        var preferences2 = new Preferences(hackathonId, new Member(1, EmployeeType.TeamLead), [1]);
        
        await service.AddPreferencesFromHackathonToDbAsync(hackathonId, preferences1);
        await service.AddPreferencesFromHackathonToDbAsync(hackathonId, preferences2);
        await service.AddTeamsFromHackathonToDbAsync(hackathonId, [team]);
        await context.Entry(hackathon).ReloadAsync();

        var updatedHackathon = await context.Entities.FirstOrDefaultAsync(e => e.HackathonId == hackathonId);

        Assert.NotNull(updatedHackathon);
        Assert.Equal(hackathonId, updatedHackathon.HackathonId);
        Assert.Contains(updatedHackathon.Teams!, t => t.Junior.Id == 1);
        Assert.Contains(updatedHackathon.Preferences!, p => p.Member.Id == 1);
    }

    [Fact(DisplayName = "Чтение информации о мероприятии из БД")]
    public async Task TestReadHackathonFromDbById()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        const int hackathonId = 13371;
        var hackathon = new Hackathon { HackathonId = hackathonId };
        await context.Entities.AddAsync(hackathon);
        await context.SaveChangesAsync();
        
        var service = new MetricCalculationService(_dbContextFactory, Mock.Of<IPublishEndpoint>());
        var retrievedHackathon = await service.GetHackathonByIdAsync(hackathonId);

        Assert.NotNull(retrievedHackathon);
        Assert.Equal(hackathonId, retrievedHackathon!.HackathonId);
    }
}