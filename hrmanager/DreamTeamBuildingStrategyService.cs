namespace hrmanager;

using domain;

public class DreamTeamBuildingStrategyService : IDreamTeamBuildingStrategyService
{
    public List<Team> CreateTeams(List<Preferences> preferences)
    {
        var juniors = preferences.Where(p => p.Member.EmployeeType == EmployeeType.Junior).ToList();
        var teamLeads = preferences.Where(p => p.Member.EmployeeType == EmployeeType.TeamLead).ToList();

        var compatibility = CalculateCompatibilityMatrix(juniors, teamLeads);

        return FormTeams(juniors, teamLeads, compatibility);
    }

    private static Dictionary<int, Dictionary<int, decimal>> CalculateCompatibilityMatrix(
        List<Preferences> juniors,
        List<Preferences> teamLeads)
    {
        var compatibility = new Dictionary<int, Dictionary<int, decimal>>();

        foreach (var junior in juniors)
        {
            var duplicateKeys = teamLeads
                .GroupBy(tl => tl.Member.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateKeys.Count != 0)
            {
                Console.WriteLine(
                    $"Были найдены дублирующие ключи: {string.Join(", ", duplicateKeys)}");
                teamLeads = teamLeads
                    .GroupBy(tl => tl.Member.Id)
                    .Select(group => group.First())
                    .ToList();
            }


            compatibility[junior.Member.Id] = teamLeads.ToDictionary(
                teamLead => teamLead.Member.Id,
                teamLead => CalculateEnjoyableMetric(junior, teamLead)
            );
        }

        return compatibility;
    }

    private static decimal CalculateEnjoyableMetric(Preferences juniorPreferences, Preferences teamLeadPreferences)
    {
        var juniorPrefs = juniorPreferences.PreferencesList;
        var teamLeadPrefs = teamLeadPreferences.PreferencesList;
        var max = teamLeadPreferences.PreferencesList.Count;

        var juniorToLead = juniorPrefs.Contains(teamLeadPreferences.Member.Id)
            ? max - juniorPrefs.IndexOf(teamLeadPreferences.Member.Id)
            : 0;

        var leadToJunior = teamLeadPrefs.Contains(juniorPreferences.Member.Id)
            ? max - teamLeadPrefs.IndexOf(juniorPreferences.Member.Id)
            : 0;

        return juniorToLead > 0 && leadToJunior > 0
            ? 1m / juniorToLead + 1m / leadToJunior
            : decimal.MinValue;
    }

    private static List<Team> FormTeams(
        List<Preferences> juniorPreferences,
        List<Preferences> teamLeadsPreferences,
        Dictionary<int, Dictionary<int, decimal>> compatibility)
    {
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();

        foreach (var junior in juniorPreferences)
        {
            var bestTeamLead = teamLeadsPreferences
                .Where(tl => !assignedTeamLeads.Contains(tl.Member.Id))
                .OrderByDescending(tl => compatibility[junior.Member.Id][tl.Member.Id])
                .FirstOrDefault();

            if (bestTeamLead == null)
            {
                continue;
            }

            teams.Add(new Team(junior.Member, bestTeamLead.Member));
            assignedTeamLeads.Add(bestTeamLead.Member.Id);
        }

        return teams;
    }
}