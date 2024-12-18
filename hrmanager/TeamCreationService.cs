namespace hrmanager;

using domain;
using System.Collections.Concurrent;

public class TeamCreationService
{
    private readonly ConcurrentDictionary<int, List<Preferences>> _hackathonIdPreferences = new();
    private const int HackathonMembersCount = 10;

    public void AddPreferences(Preferences preferences)
    {
        var hackathonId = preferences.HackathonId;

        _hackathonIdPreferences.AddOrUpdate(
            hackathonId,
            _ => [preferences],
            (_, list) =>
            {
                list.Add(preferences);
                return list;
            }
        );
    }

    public bool IsAllPreferencesForHackathonArePresented(int hackathonId)
    {
        return _hackathonIdPreferences.TryGetValue(hackathonId, out var preferences) && preferences.Count == HackathonMembersCount;
    }

    public List<Team> BuildTeams(int hackathonId)
    {
        if (!_hackathonIdPreferences.TryGetValue(hackathonId, out var preferences))
        {
            throw new InvalidOperationException("Для этого хакатона нет предпочтений.");
        }

        var teams = CreateTeams(preferences);
        _hackathonIdPreferences.TryRemove(hackathonId, out _);
        return teams;
    }

    private static List<Team> CreateTeams(List<Preferences> preferences)
    {
        var juniors = preferences.Where(p => p.Member.EmployeeType == EmployeeType.Junior).ToList();
        var teamLeads = preferences.Where(p => p.Member.EmployeeType == EmployeeType.TeamLead).ToList();

        var compatibility = CalculateCompatibilityMatrix(juniors, teamLeads);

        return MatchTeams(juniors, teamLeads, compatibility);
    }

    private static Dictionary<int, Dictionary<int, float>> CalculateCompatibilityMatrix(
        List<Preferences> juniors,
        List<Preferences> teamLeads)
    {
        var compatibility = new Dictionary<int, Dictionary<int, float>>();

        foreach (var junior in juniors)
        {
            compatibility[junior.Member.Id] = teamLeads.ToDictionary(
                teamLead => teamLead.Member.Id,
                teamLead => CalculateCompatibilityScore(junior, teamLead)
            );
        }

        return compatibility;
    }

    private static float CalculateCompatibilityScore(Preferences junior, Preferences teamLead)
    {
        var juniorPrefs = junior.PreferencesList;
        var teamLeadPrefs = teamLead.PreferencesList;
        var max = teamLead.PreferencesList.Count;

        var juniorToLead = juniorPrefs.Contains(teamLead.Member.Id)
            ? max - juniorPrefs.IndexOf(teamLead.Member.Id)
            : 0;

        var leadToJunior = teamLeadPrefs.Contains(junior.Member.Id)
            ? max - teamLeadPrefs.IndexOf(junior.Member.Id)
            : 0;

        return juniorToLead > 0 && leadToJunior > 0
            ? 1f / juniorToLead + 1f / leadToJunior
            : float.MinValue;
    }

    private static List<Team> MatchTeams(
        List<Preferences> juniors,
        List<Preferences> teamLeads,
        Dictionary<int, Dictionary<int, float>> compatibility)
    {
        //TODO: change strategy
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();
        var assignedJuniors = new HashSet<int>();

        foreach (var junior in juniors)
        {
            var bestTeamLead = teamLeads
                .Where(tl => !assignedTeamLeads.Contains(tl.Member.Id))
                .OrderByDescending(tl => compatibility[junior.Member.Id][tl.Member.Id])
                .FirstOrDefault();

            if (bestTeamLead == null)
            {
                continue;
            }

            teams.Add(new Team(junior.Member, bestTeamLead.Member));
            assignedTeamLeads.Add(bestTeamLead.Member.Id);
            assignedJuniors.Add(junior.Member.Id);
        }

        return teams;
    }
}