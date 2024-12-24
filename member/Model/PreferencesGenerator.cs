using domain;

namespace member.Model;

public class PreferencesGenerator(Member member, AllMembers allMembers)
{
    public List<int> GenerateRandomSortedPreferences()
    {
        var random = new Random();
        return member.EmployeeType == EmployeeType.Junior
            ? allMembers.TeamLead.Select(it => it.Id).OrderBy(_ => random.Next()).ToList()
            : allMembers.Junior.Select(it => it.Id).OrderBy(_ => random.Next()).ToList();
    }
}