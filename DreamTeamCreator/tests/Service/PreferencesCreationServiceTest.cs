using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;
using DreamTeamCreatorProjectTests.Mocks;

namespace DefaultNamespace;

using Xunit;

public class PreferencesCreationServiceTest
{
    [Fact]
    public void
        Test_Correct_Size_In_Generated_Preferences()
    {
        var teamLeads = CommonMocks.GetDefaultJuniors();
        var juniors = CommonMocks.GetDefaultTeamLeads();
        var preferenceService = new PreferencesCreationService();

        PotentialTeamMembersPreferences preferences =
            preferenceService.GeneratePreferences(new PotentialTeamMembers(teamLeads, juniors));

        Assert.Equal(teamLeads.Count, preferences.TeamLeadPreferences.Count);
        Assert.Equal(juniors.Count, preferences.JuniorPreferences.Count);

        foreach (var teamLeadPreference in preferences.TeamLeadPreferences)
        {
            Assert.Equal(juniors.Count, teamLeadPreference.PreferredEmployees.Count);
        }

        foreach (var juniorPreference in preferences.JuniorPreferences)
        {
            Assert.Equal(teamLeads.Count, juniorPreference.PreferredEmployees.Count);
        }
    }

    [Fact]
    public void Test_Correct_Employees_In_Generated_Preferences()
    {
        var teamLeads = CommonMocks.GetDefaultJuniors();
        var juniors = CommonMocks.GetDefaultTeamLeads();
        var preferenceService = new PreferencesCreationService();

        PotentialTeamMembersPreferences preferences =
            preferenceService.GeneratePreferences(new PotentialTeamMembers(teamLeads, juniors));

        foreach (var juniorPreference in preferences.JuniorPreferences)
        {
            foreach (var teamLead in teamLeads)
            {
                Assert.Contains(teamLead, juniorPreference.PreferredEmployees);
            }
        }

        foreach (var teamLeadPreference in preferences.TeamLeadPreferences)
        {
            foreach (var junior in juniors)
            {
                Assert.Contains(junior, teamLeadPreference.PreferredEmployees);
            }
        }
    }
}