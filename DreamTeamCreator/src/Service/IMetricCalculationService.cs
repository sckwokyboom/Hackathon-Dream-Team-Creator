using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public interface IMetricCalculationService
{
    decimal CalculateHarmonicMean(List<TeamEntity> teamEntities);
}