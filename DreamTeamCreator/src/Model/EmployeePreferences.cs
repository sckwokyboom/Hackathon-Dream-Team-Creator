namespace DreamTeamCreatorProject.Model;

public record EmployeePreferences(Employee Employee, Dictionary<Employee, int> PreferredEmployees);