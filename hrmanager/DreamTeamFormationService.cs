namespace hrmanager;

using domain;
using System.Collections.Concurrent;

public class DreamTeamFormationService(IDreamTeamBuildingStrategyService dreamTeamBuildingStrategyService)
{
    private readonly ConcurrentDictionary<int, List<Preferences>> _hackathonIdPreferences = new();
    private const int HackathonMembersCount = 10;

    public void AddPreferences(Preferences preferences)
    {
        var hackathonId = preferences.HackathonId;

        _hackathonIdPreferences.AddOrUpdate(
            hackathonId,
            _ => [preferences],
            (_, list) =>
            {
                lock (list)
                {
                    if (list.Any(p => p.Member.Id == preferences.Member.Id && p.Member.EmployeeType == preferences.Member.EmployeeType))
                    {
                        Console.WriteLine($"Предпочтения для участника с ID {preferences.Member.Id} уже добавлены.");
                    }
                    else
                    {
                        list.Add(preferences);
                        Console.WriteLine($"Текущий размер списка предпочтений: {list.Count}.");
                    }
                }

                return list;
            }
        );
    }

    public bool IsAllPreferencesForHackathonArePresented(int hackathonId)
    {
        return _hackathonIdPreferences.TryGetValue(hackathonId, out var preferences) &&
               preferences.Count == HackathonMembersCount;
    }

    public List<Team> BuildTeams(int hackathonId)
    {
        if (!_hackathonIdPreferences.TryGetValue(hackathonId, out var preferences))
        {
            throw new InvalidOperationException("Для этого хакатона нет предпочтений.");
        }

        if (!IsAllPreferencesForHackathonArePresented(hackathonId))
        {
            throw new InvalidOperationException(
                "Список с предпочтениями был сформирован некорректно или повреждён другим потоком.");
        }

        var teams = dreamTeamBuildingStrategyService.CreateTeams(preferences);
        _hackathonIdPreferences.TryRemove(hackathonId, out _);
        return teams;
    }
}