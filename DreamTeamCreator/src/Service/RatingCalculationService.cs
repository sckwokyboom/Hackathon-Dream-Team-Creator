using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public class RatingCalculationService : IRatingCalculationService
{
    public decimal CalculateHarmonicMean(List<TeamEntity> teamEntities)
    {
        int countOfTeamMembers = teamEntities.Count * 2;
        var result = (from teamEntity in teamEntities
            let juniorCoefficient = teamEntity.TeamMetrics.JuniorPriorityMetric
            let teamLeadCoefficient = teamEntity.TeamMetrics.JuniorPriorityMetric
            select (1.0m / teamLeadCoefficient) + (1.0m / juniorCoefficient)).Sum();
        return countOfTeamMembers / result;
    }
}