using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;

namespace DreamTeamCreatorProject.Core;

public class HRDirector(
    IMetricCalculationService metricCalculationService,
    HRManager hrManager
)
{
    public async Task HostHackathons(int hackathonCount, List<Employee> juniors, List<Employee> teamLeads)
    {
        decimal totalHarmonyValue = 0;
        for (var i = 1; i <= hackathonCount; i++)
        {
            Console.WriteLine($"Hackathon #{i}");
            var potentialTeamMembersPreferences =
                hrManager.GetPreferences(juniors, teamLeads);
            var teamEntities =
                hrManager.FormTeams(potentialTeamMembersPreferences.JuniorPreferences,
                        potentialTeamMembersPreferences.TeamLeadPreferences)
                    .Select(team => (TeamEntity)team).ToList();

            var harmony = metricCalculationService.CalculateHarmonicMean(teamEntities);
            totalHarmonyValue += harmony;

            var hackathon = new HackathonEntity(
                Guid.NewGuid().GetHashCode(),
                harmony,
                teamEntities
            );
            Console.WriteLine($"Hackathon Harmony: {harmony:F2}");
        }

        var averageHarmony = totalHarmonyValue / hackathonCount;
        Console.WriteLine(
            $"Average harmonical mean after host of {hackathonCount} hackathons: {averageHarmony:F2}");
    }
}