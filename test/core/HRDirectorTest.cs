namespace test;

using hrdirector;
using domain;

public class HrDirectorCoreTest
{
    [Fact(DisplayName =
        "Проверка алгоритма вычисления среднего гармонического. Среднее гармоническое одинаковых чисел равно им всем.")]
    public void TestMetricsAverage1()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Member(1, EmployeeType.Junior), new Member(1, EmployeeType.TeamLead)),
            new(new Member(2, EmployeeType.Junior), new Member(2, EmployeeType.TeamLead)),
            new(new Member(3, EmployeeType.Junior), new Member(3, EmployeeType.TeamLead)),
            new(new Member(4, EmployeeType.Junior), new Member(4, EmployeeType.TeamLead)),
            new(new Member(5, EmployeeType.Junior), new Member(5, EmployeeType.TeamLead))
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Member(1, EmployeeType.TeamLead), [1, 2, 3, 4, 5]),
            new(hackathonId, new Member(2, EmployeeType.TeamLead), [2, 1, 3, 4, 5]),
            new(hackathonId, new Member(3, EmployeeType.TeamLead), [3, 2, 1, 4, 5]),
            new(hackathonId, new Member(4, EmployeeType.TeamLead), [4, 2, 3, 1, 5]),
            new(hackathonId, new Member(5, EmployeeType.TeamLead), [5, 2, 3, 4, 1]),
            new(hackathonId, new Member(1, EmployeeType.Junior), [1, 2, 3, 4, 5]),
            new(hackathonId, new Member(2, EmployeeType.Junior), [2, 1, 3, 4, 5]),
            new(hackathonId, new Member(3, EmployeeType.Junior), [3, 2, 1, 4, 5]),
            new(hackathonId, new Member(4, EmployeeType.Junior), [4, 2, 3, 1, 5]),
            new(hackathonId, new Member(5, EmployeeType.Junior), [5, 2, 3, 4, 1]),
        };

        var harmony = MetricCalculationService.CalculateHarmony(teams, preferences);

        Assert.Equal(5.0m, harmony);
    }

    [Fact(DisplayName =
        "Проверка алгоритма вычисления среднего гармонического. Среднее гармоническое одинаковых чисел равно им всем")]
    public void TestMetricAverage2()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Member(1, EmployeeType.Junior), new Member(1, EmployeeType.TeamLead)),
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Member(1, EmployeeType.Junior), [1]),
            new(hackathonId, new Member(1, EmployeeType.TeamLead), [1]),
        };

        var harmony = MetricCalculationService.CalculateHarmony(teams, preferences);

        Assert.Equal(1.0m, harmony);
    }

    [Theory(DisplayName =
        "Проверка алгоритма вычисления среднего гармонического. Среднее гармоническое должно совпадать с заранее вычисленным.")]
    [InlineData(new[] { 2, 6 }, 3.0)]
    [InlineData(new[] { 1, 1 }, 1.0)]
    public void CalculateHarmonicMean(int[] numbers, decimal expected)
    {
        var harmony = MetricCalculationService.HarmonyImpl(numbers.ToList());
        Assert.True(Math.Abs(harmony - expected) < 0.01m);
    }

    [Fact(DisplayName =
        "Заранее определённые списки предпочтений и команды, должны дать, заранее определённое значение.")]
    public void TestMetricCalculation1()
    {
        const int hackathonId = 1;
        var teams = new List<Team>
        {
            new(new Member(1, EmployeeType.Junior), new Member(1, EmployeeType.TeamLead)),
            new(new Member(2, EmployeeType.Junior), new Member(2, EmployeeType.TeamLead)),
            new(new Member(3, EmployeeType.Junior), new Member(3, EmployeeType.TeamLead)),
            new(new Member(4, EmployeeType.Junior), new Member(4, EmployeeType.TeamLead)),
            new(new Member(5, EmployeeType.Junior), new Member(5, EmployeeType.TeamLead))
        };
        var preferences = new List<Preferences>
        {
            new(hackathonId, new Member(5, EmployeeType.Junior), [1, 2, 3, 4, 5]),
            new(hackathonId, new Member(4, EmployeeType.Junior), [2, 1, 3, 4, 5]),
            new(hackathonId, new Member(3, EmployeeType.Junior), [3, 2, 1, 4, 5]),
            new(hackathonId, new Member(2, EmployeeType.Junior), [4, 2, 3, 1, 5]),
            new(hackathonId, new Member(1, EmployeeType.Junior), [5, 2, 3, 4, 1]),
            new(hackathonId, new Member(5, EmployeeType.TeamLead), [1, 2, 3, 4, 5]),
            new(hackathonId, new Member(4, EmployeeType.TeamLead), [2, 1, 3, 4, 5]),
            new(hackathonId, new Member(3, EmployeeType.TeamLead), [3, 2, 1, 4, 5]),
            new(hackathonId, new Member(2, EmployeeType.TeamLead), [4, 2, 3, 1, 5]),
            new(hackathonId, new Member(1, EmployeeType.TeamLead), [5, 2, 3, 4, 1])
        };

        var harmony = MetricCalculationService.CalculateHarmony(teams, preferences);

        Assert.True(Math.Abs(harmony - 1.6949152542372881m) < 0.01m);
    }
}