using domain;

namespace member.Model;

public record AllMembers(List<Member> Junior, List<Member> TeamLead);

public class UserRecord
{
    public int Id { get; set; }
    public string Name { get; set; }
}