using DreamTeamCreatorProject.Service;

namespace DreamTeamCreatorProjectTests.Mocks;

using DreamTeamCreatorProject.Core;
using DreamTeamCreatorProject.Model;
using Moq;

public static class CommonMocks
{
    public static List<Employee> GetDefaultTeamLeads()
    {
        var teamLeads = new List<Employee>
            { new(1, "John Snow"), new(2, "Daenerys Targaryen") };
        return teamLeads;
    }

    public static List<Employee> GetDefaultJuniors()
    {
        var juniors = new List<Employee>
            { new(3, "Dolorous Edd"), new(4, "Jorah Mormont") };
        return juniors;
    }

    public static List<TeamEntity> GetIdenticalTeams()
    {
        return
        [
            new TeamEntity(new Employee(1, "John Snow"), new Employee(3, "Dolorous Edd"),
                new TeamMetrics(4, 4)),

            new TeamEntity(new Employee(2, "Daenerys Targaryen"), new Employee(4, "Jorah Mormont"),
                new TeamMetrics(4, 4))
        ];
    }


    public static List<TeamEntity> GetDefaultExpectedTeams(List<Employee> teamLeads,
        List<Employee> juniors)
    {
        var expectedTeams = new List<TeamEntity>
        {
            new(teamLeads[0], juniors[0], new TeamMetrics(1, 1)),
            new(teamLeads[1], juniors[1], new TeamMetrics(1, 1))
        };
        return expectedTeams;
    }

    public static List<EmployeePreferences> GetTeamLeadsPreferencesList(
        List<Employee> teamLeads, List<Employee> juniors)
    {
        var teamLeadsPreferencesList = new List<EmployeePreferences>
        {
            new(teamLeads[0],
                new Dictionary<Employee, int>
                    { { juniors[0], 1 }, { juniors[1], 2 } }),
            new(teamLeads[1],
                new Dictionary<Employee, int>
                    { { juniors[1], 1 }, { juniors[0], 2 } })
        };
        return teamLeadsPreferencesList;
    }

    public static List<EmployeePreferences> GetEmployeePreferencesList(
        List<Employee> juniors, List<Employee> teamLeads)
    {
        var juniorsPreferencesList = new List<EmployeePreferences>
        {
            new(juniors[0],
                new Dictionary<Employee, int>
                    { { teamLeads[0], 1 }, { teamLeads[1], 2 } }),
            new(juniors[1],
                new Dictionary<Employee, int>
                    { { teamLeads[1], 1 }, { teamLeads[0], 2 } })
        };
        return juniorsPreferencesList;
    }

    public static HRManager MockHRManager(List<Employee> juniors,
        List<Employee> teamLeads,
        List<EmployeePreferences> juniorsPreferences,
        List<EmployeePreferences> teamLeadsPreferences,
        List<TeamEntity> expectedTeams)
    {
        var mockPreferencesCreationService = MockPreferencesCreationService(juniors, teamLeads,
            juniorsPreferences, teamLeadsPreferences);
        var mockTeamFormationService = MockTeamBuildingStrategyService(expectedTeams);
        var mockHackathon =
            new Mock<Hackathon>(mockPreferencesCreationService.Object);

        var hrManager = new HRManager(mockTeamFormationService.Object,
            mockHackathon.Object);
        return hrManager;
    }

    public static Mock<ITeamBuildingStrategy> MockTeamBuildingStrategyService(
        List<TeamEntity> expectedTeams)
    {
        var mockTeamFormationService = new Mock<ITeamBuildingStrategy>();

        mockTeamFormationService.Setup(s =>
                s.BuildTeams(It.IsAny<List<Employee>>(), It.IsAny<List<Employee>>(),
                    It.IsAny<List<Wishlist>>(), It.IsAny<List<Wishlist>>()))
            .Returns(expectedTeams);
        return mockTeamFormationService;
    }

    public static Mock<IPreferencesCreationService> MockPreferencesCreationService(
        List<Employee> juniors, List<Employee> teamLeads,
        List<EmployeePreferences> juniorsPreferencesList,
        List<EmployeePreferences> teamLeadsPreferencesList)
    {
        var mockPreferencesService = new Mock<IPreferencesCreationService>();

        mockPreferencesService
            .Setup(h => h.GeneratePreferences(new PotentialTeamMembers(teamLeads, juniors)))
            .Returns(new PotentialTeamMembersPreferences(teamLeadsPreferencesList, juniorsPreferencesList));
        return mockPreferencesService;
    }
}