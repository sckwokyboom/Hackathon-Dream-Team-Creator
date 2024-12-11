using DreamTeamCreatorProject.Core;
using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;
using DreamTeamCreatorProjectTests.Mocks;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace DefaultNamespace;

public class HRDirectorTest
{
    [Fact]
    public void Test_Harmonic_Mean_Of_Identical_Basic()
    {
        var service = new MetricCalculationService();
        var identicalTeams = CommonMocks.GetIdenticalTeams();

        var harmony = service.CalculateHarmonicMean(identicalTeams);

        Assert.Equal(4.0m, harmony, precision: 2);
    }

    [Fact]
    public void Test_Harmonic_Mean_Basic()
    {
        var service = new MetricCalculationService();
        var predefinedTeams = new List<TeamEntity>
        {
            new(new Employee(1, "John"), new Employee(2, "Edd"),
                new TeamMetrics(2, 6)),
        };

        var harmonicMeanValue = service.CalculateHarmonicMean(predefinedTeams);

        Assert.Equal(3.0m, harmonicMeanValue, precision: 2);
    }

    [Fact]
    public void Test_Predefined_Teams_Should_Return_Predefined_Harmony_Value()
    {
        var service = new MetricCalculationService();

        var predefinedTeams = new List<TeamEntity>
        {
            new(new Employee(1, "John"), new Employee(2, "Edd"),
                new TeamMetrics(8, 8)),
            new(new Employee(3, "Dany"), new Employee(4, "Jorah"),
                new TeamMetrics(8, 6)),
            new(new Employee(5, "Dany"), new Employee(6, "Jorah"),
                new TeamMetrics(6, 6)),
            new(new Employee(7, "Dany"), new Employee(8, "Jorah"),
                new TeamMetrics(2, 6))
        };

        // It should be equal: 8 / (1/8 + 1/8 + 1/8 + 1/6 + 1/6 + 1/6 + 1/2 + 1/6) == 5.189 == 5.19
        var harmonicMeanValue = service.CalculateHarmonicMean(predefinedTeams);
        Assert.Equal(5.19m, harmonicMeanValue, precision: 2);
    }

    [Fact]
    public void Test_HR_Manager_Should_Be_Called_Only_Once_Per_Hackathon()
    {
        var juniors = CommonMocks.GetDefaultJuniors();
        var teamLeads = CommonMocks.GetDefaultTeamLeads();

        var expectedTeams = CommonMocks.GetDefaultExpectedTeams(teamLeads, juniors);
        var juniorsPreferencesList =
            CommonMocks.GetEmployeePreferencesList(juniors, teamLeads);

        var teamLeadsPreferencesList =
            CommonMocks.GetTeamLeadsPreferencesList(teamLeads, juniors);
        var hrManager = CommonMocks.MockHRManager(juniors, teamLeads,
            juniorsPreferencesList, teamLeadsPreferencesList, expectedTeams);

        var service = new Mock<IMetricCalculationService>();
        service.Setup(s =>
                s.CalculateHarmonicMean(expectedTeams))
            .Returns(3);
        var hrDirector = new HRDirector(service.Object, hrManager);

        hrDirector.HostHackathons(10, juniors, teamLeads);

        service.Verify(m => m.CalculateHarmonicMean(expectedTeams),
            Times.Exactly(10));
    }

    // [Fact]
    // public void
    //     Hackathon_With_Predefined_Participants_And_Preferences_Should_Give_Expected_Harmony()
    // {
    //     var teamLead = new Employee(1, "TeamLead");
    //     var junior = new Employee(2, "Junior");
    //
    //     var teamLeadPreferences = new Dictionary<Employee, int>
    //     {
    //         { junior, 1 }
    //     };
    //
    //     var juniorPreferences = new Dictionary<Employee, int>
    //     {
    //         { teamLead, 1 }
    //     };
    //
    //     var juniorPrefMock =
    //         new Mock<EmployeePreferences>(junior, juniorPreferences);
    //     var teamLeadPrefMock =
    //         new Mock<EmployeePreferences>(teamLead, teamLeadPreferences);
    //
    //     var juniorsPreferencesList = new List<EmployeePreferences>
    //         { juniorPrefMock.Object };
    //     var teamLeadsPreferencesList = new List<EmployeePreferences>
    //         { teamLeadPrefMock.Object };
    //
    //     var preferenceServiceMock = new Mock<IPreferencesCreationService>();
    //     preferenceServiceMock.Setup(h =>
    //             h.GeneratePreferences(It.IsAny<PotentialTeamMembers>()))
    //         .Returns(new PotentialTeamMembersPreferences(juniorsPreferencesList, teamLeadsPreferencesList));
    //     var mockHackathon = new Hackathon(preferenceServiceMock.Object);
    //
    //     var teamFormationService = new TeamBuildingStrategy();
    //
    //     var hrManager =
    //         new HRManager(teamFormationService, mockHackathon);
    //     var ratingService = new RatingCalculationService();
    //
    //     var hrDirector = new HRDirector(ratingService, hrManager);
    //
    //     var teams = hrManager.FormTeams(juniorsPreferencesList, teamLeadsPreferencesList)
    //         .Select(team => (TeamEntity)team).ToList();
    //     var harmony = ratingService.CalculateHarmonicMean(teams);
    //
    //     Assert.Equal(1.0m, harmony);
    // }
}