using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;

namespace DreamTeamCreatorProject.Core;

public class Hackathon(IPreferencesCreationService preferencesService)
{
    public PotentialTeamMembersPreferences GeneratePreferences(List<Employee> juniors, List<Employee> teamLeads)
    {
        return preferencesService.GeneratePreferences(new PotentialTeamMembers(teamLeads, juniors));
    }
}