namespace hrdirector;

using domain;
using MassTransit;

public class PreferencesResponseConsumer(
    MetricCalculationService metricCalculationService) : IConsumer<Preferences>
{
    public async Task Consume(ConsumeContext<Preferences> context)
    {
        await metricCalculationService.AddPreferencesFromHackathonToDbAsync(context.Message.HackathonId, context.Message);
    }
}