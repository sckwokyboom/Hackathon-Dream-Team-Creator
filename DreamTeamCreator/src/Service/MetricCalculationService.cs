using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public class MetricCalculationService : IMetricCalculationService
{
    private const int CountOfMembersInTeam = 2;

    public decimal CalculateHarmonicMean(List<TeamEntity> teamEntities)
    {
        var countOfTeamMembers = teamEntities.Count * CountOfMembersInTeam;
        var result = (from teamEntity in teamEntities
            let juniorCoefficient = teamEntity.TeamMetrics.JuniorPriorityMetric
            let teamLeadCoefficient = teamEntity.TeamMetrics.TeamLeadPriorityMetric
            select (1.0m / teamLeadCoefficient) + (1.0m / juniorCoefficient)).Sum();
        return countOfTeamMembers / result;
    }
}