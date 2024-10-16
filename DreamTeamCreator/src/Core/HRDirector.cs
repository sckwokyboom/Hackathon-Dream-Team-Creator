using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;

namespace DreamTeamCreatorProject.Core;

public class HRDirector(
    IRatingCalculationService ratingCalculationService,
    HRManager hrManager
)
{
    public async Task OverseeHackathons(int hackathonCount, List<Employee> juniors, List<Employee> teamLeads)
    {
        decimal totalHarmonyValue = 0;
        for (var i = 1; i <= hackathonCount; i++)
        {
            Console.WriteLine($"--- Hackathon #{i} ---");

            var potentialTeamMembersPreferences =
                hrManager.GetPreferences(juniors, teamLeads);
            var teamEntities =
                hrManager.FormTeams(potentialTeamMembersPreferences.JuniorPreferences,
                        potentialTeamMembersPreferences.TeamLeadPreferences)
                    .Select(team => (TeamEntity)team).ToList();

            var harmony = ratingCalculationService.CalculateHarmonicMean(teamEntities);
            totalHarmonyValue += harmony;

            var hackathon = new HackathonEntity(
                Guid.NewGuid().GetHashCode(),
                harmony,
                teamEntities
            );

            // await hackathonRepository.AddHackathonAsync(hackathon);
            // await hackathonRepository.SavePreferencesAsync(juniorPreferences,
            //     Role.Junior, hackathon.Id);
            // await hackathonRepository.SavePreferencesAsync(teamLeadPreferences,
            //     Role.TeamLead, hackathon.Id);

            Console.WriteLine($"Hackathon Harmony: {harmony:F2}");
        }

        var averageHarmony = totalHarmonyValue / hackathonCount;
        Console.WriteLine(
            $"Average Harmony after {hackathonCount} Hackathons: {averageHarmony:F2}");
    }
    
    // public async Task PrintHackathonResultsAsync(int hackathonId)
    // {
    //     var hackathon =
    //         await hackathonRepository.GetHackathonByIdAsync(hackathonId);
    //
    //     Console.WriteLine($"Hackathon ID: {hackathon.Id}");
    //     Console.WriteLine($"Harmony: {hackathon.Harmony:F2}");
    //
    //     var participants = new HashSet<string>();
    //
    //     foreach (var team in hackathon.Teams)
    //     {
    //         var teamLeadName =
    //             await GetEmployeeByIdAsync(team.TeamLeadId, Role.TeamLead);
    //         var juniorName =
    //             await GetEmployeeByIdAsync(team.JuniorId, Role.Junior);
    //
    //         Console.WriteLine(
    //             $"Team Lead: {teamLeadName}, Junior: {juniorName}");
    //         participants.Add(teamLeadName);
    //         participants.Add(juniorName);
    //     }
    //
    //     Console.WriteLine("\nParticipants:");
    //     foreach (var participant in participants)
    //     {
    //         Console.WriteLine($"- {participant}");
    //     }
    // }
    //
    // public async Task PrintAverageHarmonyAsync()
    // {
    //     var averageHarmony =
    //         await hackathonRepository.CalculateAverageHarmonyAsync();
    //     Console.WriteLine(
    //         $"Average Harmony across all hackathons: {averageHarmony:F2}");
    // }
    //
    //
    // private async Task<string> GetEmployeeByIdAsync(int employeeId, Role role)
    // {
    //     var employee =
    //         await employeeRepository.GetEmployeeByIdAsync(employeeId, role);
    //     return employee?.Name ?? "Unknown";
    // }
}