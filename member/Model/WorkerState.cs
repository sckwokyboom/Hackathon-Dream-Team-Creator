using domain;

namespace member.Model;

public abstract class WorkerState
{
    private WorkerState()
    {
    }

    public sealed class Empty : WorkerState
    {
        public static readonly Empty Instance = new();

        private Empty()
        {
        }
    }

    public sealed class MemberState(Member member, AllMembers allMembers) : WorkerState
    {
        public Member Member { get; } = member;
        public AllMembers AllMembers { get; } = allMembers;
        public PreferencesGenerator PreferencesGenerator { get; } = new(member, allMembers);
    }
}