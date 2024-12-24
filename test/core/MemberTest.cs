using domain;
using member.Model;

namespace test;

public class MemberTest
{
    private const int HackathonMemberOneTypeEmployeeCount = 5;

    private static List<Member> GenerateEmployees(EmployeeType type, int count)
    {
        return Enumerable.Range(0, count)
            .Select(i => new Member(i, type))
            .ToList();
    }

    [Fact(DisplayName = "Генерация WishList: Размер списка должен совпадать с количеством тимлидов/джунов.")]
    public void EmployeeContainsTest()
    {
        var allMembers = CreateAllMembers(out var juniors, out var teamleads);
        var generator = new PreferencesGenerator(juniors[0], allMembers);

        var preferences = generator.GenerateRandomSortedPreferences();

        Assert.Equal(juniors.Count, preferences.Count);
        Assert.Equal(teamleads.Count, preferences.Count);
    }

    [Fact(DisplayName = "Генерация WishList: Заранее определённый сотрудник должен присутствовать в списке.")]
    public void CorrectIdsTest()
    {
        var allMembers = CreateAllMembers(out var juniors, out var teamLeads);
        var generator = new PreferencesGenerator(juniors[0], allMembers);
        var expected = teamLeads.Select(it => it.Id).OrderBy(id => id).ToList();

        var preferences = generator.GenerateRandomSortedPreferences();

        Assert.Equal(expected, preferences.OrderBy(id => id).ToList());
    }

    private static AllMembers CreateAllMembers(out List<Member> juniors, out List<Member> teamLeads)
    {
        juniors = GenerateEmployees(EmployeeType.Junior, HackathonMemberOneTypeEmployeeCount);
        teamLeads = GenerateEmployees(EmployeeType.TeamLead, HackathonMemberOneTypeEmployeeCount);
        return new AllMembers(juniors, teamLeads);
    }
}