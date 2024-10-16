using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public interface IRatingCalculationService
{
    decimal CalculateHarmonicMean(List<TeamEntity> teamEntities);
}