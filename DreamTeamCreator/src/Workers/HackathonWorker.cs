using System.ComponentModel;
using DreamTeamCreatorProject.Core;
using DreamTeamCreatorProject.Service;
using Microsoft.Extensions.Hosting;

namespace DreamTeamCreatorProject.Workers;

public class HackathonWorker(
    HRDirector hrDirector,
    EmployeesLoaderService employeesLoaderService,
    IHostApplicationLifetime hostApplicationLifetime)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var juniors = EmployeesLoaderService.GetJuniors();
        var teamLeads = EmployeesLoaderService.GetTeamLeads();

        await hrDirector.HostHackathons(1000, juniors, teamLeads);
        hostApplicationLifetime.StopApplication();
    }


}