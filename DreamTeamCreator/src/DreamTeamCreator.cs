// See https://aka.ms/new-console-template for more information

using DreamTeamCreatorProject.Core;
using DreamTeamCreatorProject.Model;
using DreamTeamCreatorProject.Service;
using DreamTeamCreatorProject.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class DreamTeamCreator
{
    public static void Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args).ConfigureLogging(
                (context, logging) =>
                {
                    var config = context.Configuration.GetSection("Logging");
                    logging.AddConfiguration(config);
                    logging.AddConsole();
                    logging.AddFilter(
                        "Microsoft.EntityFrameworkCore.Database.Command",
                        LogLevel.Warning);
                })
            .ConfigureServices((_, services) =>
            {
                services.AddHostedService<HackathonWorker>();
                services.AddTransient<EmployeesLoaderService>();

                services
                    .AddTransient<IPreferencesCreationService, PreferencesCreationService>();
                services
                    .AddTransient<ITeamBuildingStrategy,
                        TeamBuildingStrategy>();
                services.AddTransient<IMetricCalculationService, MetricCalculationService>();

                services.AddTransient<Hackathon>();
                services.AddTransient<HRManager>();
                services.AddTransient<HRDirector>();
            })
            .Build();

        host.Run();
    }
}