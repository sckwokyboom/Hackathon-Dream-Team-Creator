namespace DreamTeamCreatorProject.Model;

public record TeamEntity(Employee TeamLead, Employee Junior, TeamMetrics TeamMetrics): Team(TeamLead, Junior);