using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public class PreferencesCreationService : IPreferencesCreationService
{
    public PotentialTeamMembersPreferences GeneratePreferences(PotentialTeamMembers potentialTeamMembers)
    {
        var teamLeadsAndTheirPreferredJuniors =
            CreateRandomTeamMemberPreferences(potentialTeamMembers.TeamLeads, potentialTeamMembers.Juniors);
        var juniorsAndTheirPreferredTeamLeads =
            CreateRandomTeamMemberPreferences(potentialTeamMembers.Juniors, potentialTeamMembers.TeamLeads);

        return new PotentialTeamMembersPreferences(
            teamLeadsAndTheirPreferredJuniors,
            juniorsAndTheirPreferredTeamLeads);
    }

    private List<EmployeePreferences> CreateRandomTeamMemberPreferences(List<Employee> employees,
        List<Employee> randomOrderPreferencesEmployees)
    {
        List<EmployeePreferences> result = [];
        var countOfPreferredEmployees = randomOrderPreferencesEmployees.Count;
        foreach (var employee in employees)
        {
            var orderedPreferences =
                randomOrderPreferencesEmployees.OrderBy(x => Guid.NewGuid()).ToList();
            var preferredEmployees = orderedPreferences.Select((preference, index) =>
                    new
                    {
                        preference,
                        priority = countOfPreferredEmployees - index
                    })
                .ToDictionary(entry => entry.preference, entry => entry.priority);
            result.Add(new EmployeePreferences(employee, preferredEmployees));
        }

        return result;
    }
}