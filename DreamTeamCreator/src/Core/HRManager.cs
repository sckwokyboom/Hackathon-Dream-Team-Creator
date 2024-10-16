using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;

namespace DreamTeamCreatorProject.Core;

public class HRManager(
    ITeamBuildingStrategy teamBuildingStrategy,
    Hackathon hackathon)
{
    public PotentialTeamMembersPreferences GetPreferences(List<Employee> juniors, List<Employee> teamLeads)
    {
        return
            hackathon.GeneratePreferences(juniors, teamLeads);
    }

    public IEnumerable<Team> FormTeams(List<EmployeePreferences> juniorPreferences,
        List<EmployeePreferences> teamLeadPreferences)
    {
        List<Employee> juniors = juniorPreferences.Select(pref => pref.Employee).ToList();
        List<Employee> teamLeads = teamLeadPreferences.Select(pref => pref.Employee).ToList();
        List<Wishlist> juniorsWishLists = juniorPreferences
            .Select(pref =>
                new Wishlist(pref.Employee.Id, pref.PreferredEmployees.Keys.Select(employee => employee.Id).ToArray()))
            .ToList();
        List<Wishlist> teamLeadsWishLists = juniorPreferences
            .Select(pref =>
                new Wishlist(pref.Employee.Id, pref.PreferredEmployees.Keys.Select(employee => employee.Id).ToArray()))
            .ToList();


        return teamBuildingStrategy.BuildTeams(teamLeads, juniors, teamLeadsWishLists, juniorsWishLists);
    }
}