using domain;

namespace test;

using hrmanager;
using Moq;

public class HrManagerTest
{
    private static List<Member> PrepareEmployees(int juniorCount, int teamLeadCount)
    {
        var employees = new List<Member>();

        for (var i = 0; i < juniorCount; i++)
        {
            employees.Add(new Member(i, EmployeeType.Junior));
        }

        for (var i = 0; i < teamLeadCount; i++)
        {
            employees.Add(new Member(i, EmployeeType.TeamLead));
        }

        return employees;
    }

    [Fact(DisplayName = "Количество команд должно совпадать с заранее заданным.")]
    public void ShouldMatchExpectedTeamCount()
    {
        const int hackathonId = 1;
        var service = new DreamTeamFormationService(new DreamTeamBuildingStrategyService());
        var employees = PrepareEmployees(5, 5);
        foreach (var employee in employees)
        {
            var preferences = new Preferences(hackathonId, employee, [5, 4, 3, 1, 2]);
            service.AddPreferences(preferences);
        }

        var teams = service.BuildTeams(hackathonId);

        Assert.Equal(employees.Count / 2, teams.Count);
    }

    [Fact(DisplayName =
        "Стратегия HRManager'а – на заранее определённых предпочтениях, должна возвращать одинаковое распределение.")]
    public void ShouldReturnSameTeamDistribution()
    {
        const int hackathonId = 1;
        var teamsList = new List<List<Team>>();

        for (var j = 0; j < 3; j++)
        {
            var service = new DreamTeamFormationService(new DreamTeamBuildingStrategyService());
            var employees = PrepareEmployees(5, 5);
            foreach (var employee in employees)
            {
                var preferences = new Preferences(hackathonId, employee, [1, 2, 3, 4, 5]);
                service.AddPreferences(preferences);
            }

            var teams = service.BuildTeams(hackathonId);
            teamsList.Add(teams);
        }

        Assert.All(teamsList, team => team.Equals(teamsList[0]));
    }


    [Fact(DisplayName = "Стратегия HRManager-а должна быть вызвана ровно один раз.")]
    public void ShouldThrowOnSecondTeamBuildAttempt()
    {
        const int hackathonId = 1;
        var mock = new Mock<IDreamTeamBuildingStrategyService>();
        var service = new DreamTeamFormationService(mock.Object);
        var employees = PrepareEmployees(5, 5);
        foreach (var employee in employees)
        {
            var preferences = new Preferences(hackathonId, employee, [4, 1, 2, 5, 3]);
            service.AddPreferences(preferences);
        }

        service.BuildTeams(hackathonId);

        mock.Verify(m => m.CreateTeams(It.IsAny<List<Preferences>>()), Times.Once);
    }
}