using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public class TeamBuildingStrategy : ITeamBuildingStrategy
{
    public IEnumerable<Team> BuildTeams(IEnumerable<Employee> teamLeads, IEnumerable<Employee> juniors,
        IEnumerable<Wishlist> teamLeadsWishlists, IEnumerable<Wishlist> juniorsWishlists)
    {
        var teamLeadPreferences = teamLeadsWishlists
            .Select(wishList =>
                new EmployeePreferences(teamLeads.First(teamLead => teamLead.Id == wishList.EmployeeId),
                    wishList.DesiredEmployeesIds
                        .Select((desiredId, index) => new { index, desiredId })
                        .ToDictionary(
                            x => new Employee(x.desiredId, juniors.First(junior => junior.Id == x.desiredId).Name),
                            x => wishList.DesiredEmployeesIds.Length - x.index))
            );
        var juniorPreferences = juniorsWishlists
            .Select(wishList =>
                new EmployeePreferences(juniors.First(junior => junior.Id == wishList.EmployeeId),
                    wishList.DesiredEmployeesIds
                        .Select((desiredId, index) => new { index, desiredId })
                        .ToDictionary(
                            x => new Employee(x.desiredId, teamLeads.First(junior => junior.Id == x.desiredId).Name),
                            x => wishList.DesiredEmployeesIds.Length - x.index))
            );
        return CreateTeams(
            new PotentialTeamMembersPreferences(teamLeadPreferences.ToList(), juniorPreferences.ToList()));
    }

    private List<TeamEntity> CreateTeams(PotentialTeamMembersPreferences potentialTeamMembersPreferences)
    {
        var freeJuniorsPreferences =
            new HashSet<EmployeePreferences>(potentialTeamMembersPreferences.JuniorPreferences);
        var teamLeadsAndJuniorMatches = new Dictionary<Employee, Employee>();
        Dictionary<Employee, Queue<Employee>> juniorAndOrderedPreferredTeamLeads =
            potentialTeamMembersPreferences.JuniorPreferences.ToDictionary(
                juniorPreference => juniorPreference.Employee,
                juniorPreference => new Queue<Employee>(juniorPreference.PreferredEmployees.Keys));

        while (freeJuniorsPreferences.Count > 0)
        {
            var juniorPref = freeJuniorsPreferences.First();
            var searchingJunior = juniorPref.Employee;
            freeJuniorsPreferences.Remove(juniorPref);

            if (juniorAndOrderedPreferredTeamLeads[juniorPref.Employee].Count == 0)
            {
                continue;
            }

            var mostPreferredTeamLead = juniorAndOrderedPreferredTeamLeads[searchingJunior].Dequeue();

            if (!teamLeadsAndJuniorMatches.TryGetValue(mostPreferredTeamLead, out var value))
            {
                teamLeadsAndJuniorMatches[mostPreferredTeamLead] = searchingJunior;
            }
            else
            {
                var currentJuniorFromTeamLeadMatch = value;
                var teamLeadPref =
                    potentialTeamMembersPreferences
                        .TeamLeadPreferences
                        .First(preference =>
                            preference.Employee == mostPreferredTeamLead);

                if (teamLeadPref.PreferredEmployees[currentJuniorFromTeamLeadMatch] >
                    teamLeadPref.PreferredEmployees[searchingJunior])
                {
                    freeJuniorsPreferences.Add(juniorPref);
                }
                else
                {
                    freeJuniorsPreferences.Add(potentialTeamMembersPreferences.JuniorPreferences.First(
                        juniorPreference =>
                            juniorPreference.Employee == currentJuniorFromTeamLeadMatch));
                    teamLeadsAndJuniorMatches[mostPreferredTeamLead] = juniorPref.Employee;
                }
            }
        }

        return (from entry in teamLeadsAndJuniorMatches
                let teamLead = entry.Key
                let junior = entry.Value
                let juniorPref =
                    potentialTeamMembersPreferences.JuniorPreferences.First(juniorPreference =>
                        juniorPreference.Employee == junior)
                let teamLeadPref =
                    potentialTeamMembersPreferences.TeamLeadPreferences.First(teamLeadPreference =>
                        teamLeadPreference.Employee == teamLead)
                let teamLeadPriority = teamLeadPref.PreferredEmployees[junior]
                let juniorPriority = juniorPref.PreferredEmployees[teamLead]
                select new TeamEntity(teamLead, junior, new TeamMetrics(teamLeadPriority, juniorPriority)))
            .ToList();
    }
}