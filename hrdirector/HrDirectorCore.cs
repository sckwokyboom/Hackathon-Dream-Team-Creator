﻿using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace hrdirector;

using domain;

public class HrDirectorCore(
    MetricCalculationService metricsCalculationService) : BackgroundService
{
    private const int TotalHackathons = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitializeHttpServer(stoppingToken);

        await metricsCalculationService.StartHackathonsAsync(TotalHackathons);
    }

    private void InitializeHttpServer(CancellationToken stoppingToken)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy());
        var app = builder.Build();

        app.MapPost("/teams", TeamsRequestHandler);

        app.MapGet("/hackathon", async (HttpContext context) =>
        {
            if (!context.Request.Query.TryGetValue("id", out var idValue) ||
                !int.TryParse(idValue, out var hackathonId))
            {
                return Results.BadRequest(new { Message = "Parameter 'id' is required and must be an integer." });
            }

            var hackathon = await metricsCalculationService.GetHackathonByIdAsync(hackathonId);
            return hackathon != null
                ? Results.Json(hackathon)
                : Results.NotFound(new { Message = $"Хакатона с ID={hackathonId} не существует." });
        });

        app.MapGet("/all-hackathons", () =>
        {
            try
            {
                return Results.Json(metricsCalculationService.GetAllHackathonIds());
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Error retrieving hackathons", Error = ex.Message });
            }
        });

        app.MapGet("/avg-harmony", () =>
        {
            try
            {
                return Results.Json(metricsCalculationService.GetAverageHarmonyAsync());
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Message = "Error calculating average harmony", Error = ex.Message });
            }
        });
        
        app.MapHealthChecks("/health");

        app.RunAsync(stoppingToken);
    }

    private async Task<IResult> TeamsRequestHandler(CreatedTeams request)
    {
        try
        {
            Console.WriteLine($"Хакатон завершен: {request.HackathonId}.");
            Console.WriteLine("Сформированные команды:");
            foreach (var team in request.Teams)
            {
                Console.WriteLine($"{team.TeamLead} - {team.Junior}");
            }

            await metricsCalculationService.AddTeamsFromHackathonToDbAsync(request.HackathonId, request.Teams);
            Console.WriteLine($"Общее среднее по гармонии из базы данных: {await metricsCalculationService.GetAverageHarmonyAsync()}");
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Message = "Error processing teams", Error = ex.Message });
        }
    }
}