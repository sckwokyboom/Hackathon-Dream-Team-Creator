using DreamTeamCreatorProject.Model;

namespace DreamTeamCreatorProject.Service;

public class EmployeesLoaderService
{
    private static string JUNIORS_CSV_PATH = "C:\\Users\\sckwo\\RiderProjects\\DreamTeamCreatorProject\\DreamTeamCreator\\src\\resources\\Juniors20.csv";
    private static string TEAMLEADS_CSV_PATH = "C:\\Users\\sckwo\\RiderProjects\\DreamTeamCreatorProject\\DreamTeamCreator\\src\\resources\\Teamleads20.csv";

    public static List<Employee> GetJuniors()
    {
        return ReadEmployees(JUNIORS_CSV_PATH);
    }

    public static List<Employee> GetTeamLeads()
    {
        return ReadEmployees(TEAMLEADS_CSV_PATH);
    }

    private static List<Employee> ReadEmployees(string filePath)
    {
        var employees = new List<Employee>();
        var lines = File.ReadAllLines(filePath);
        const int numOfColumns = 2;
        foreach (var line in lines)
        {
            var values = line.Split(';');
            if (values.Length < numOfColumns || !int.TryParse(values[0], out var id))
                continue;
            var name = values[1];
            employees.Add(new Employee(id, name));
        }

        return employees;
    }
}