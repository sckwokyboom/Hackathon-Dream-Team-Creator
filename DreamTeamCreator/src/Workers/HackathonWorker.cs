using DreamTeamCreatorProject.Core;
using DreamTeamCreatorProject.Service;
using Microsoft.Extensions.Hosting;

namespace DreamTeamCreatorProject.Workers;

public class HackathonWorker(
    HRDirector hrDirector,
    EmployeesLoaderService employeesLoaderService)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // await employeeLoaderService.SaveEmployeesFromCsvAsync(
        //     CsvFilePaths.JuniorsCsvPath, CsvFilePaths.TeamLeadsCsvPath);
        var juniors = EmployeesLoaderService.GetJuniors();
        var teamLeads = EmployeesLoaderService.GetTeamLeads();

        await hrDirector.OverseeHackathons(1000, juniors, teamLeads);
        // await hrDirector.PrintHackathonResultsAsync(6);
        // await hrDirector.PrintAverageHarmonyAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}