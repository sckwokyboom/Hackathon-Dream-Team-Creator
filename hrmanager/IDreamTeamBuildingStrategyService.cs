using domain;

namespace hrmanager;

public interface IDreamTeamBuildingStrategyService
{
    public List<Team> CreateTeams(List<Preferences> preferences);
}