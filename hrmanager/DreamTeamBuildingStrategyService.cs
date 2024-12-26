namespace hrmanager;

using domain;

public class DreamTeamBuildingStrategyService : IDreamTeamBuildingStrategyService
{
    public List<Team> CreateTeams(List<Preferences> preferences)
    {
        var juniors = preferences.Where(p => p.Member.EmployeeType == EmployeeType.Junior).ToList();
        var teamLeads = preferences.Where(p => p.Member.EmployeeType == EmployeeType.TeamLead).ToList();

        ValidatePreferences(juniors, teamLeads);

        var compatibility = CalculateCompatibilityMatrix(juniors, teamLeads);

        var strategies = new List<Func<List<Team>>>
        {
            () => GreedyStrategy(juniors, teamLeads, compatibility),
            () => GlobalOptimizationStrategy(juniors, teamLeads, compatibility),
            () => StochasticStrategy(juniors, teamLeads, compatibility, 30, preferences),
            () => HungarianAlgorithmStrategy(juniors, teamLeads, compatibility)
        };

        // Выбор лучшей стратегии
        var bestTeams = strategies
            .Select(strategy => strategy.Invoke())
            .OrderByDescending(teams => CalculateHarmony(teams, preferences))
            .First();

        return bestTeams;
    }


    private static void ValidatePreferences(List<Preferences> juniors, List<Preferences> teamLeads)
    {
        if (juniors.Count != teamLeads.Count)
        {
            throw new InvalidOperationException(
                "Количество предпочтений juniors не совпадает с количеством предпочтений team leads.");
        }

        var duplicateKeysTeamLeads = teamLeads
            .GroupBy(tl => tl.Member.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        var duplicateKeysJuniors = juniors
            .GroupBy(tl => tl.Member.Id)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();
        if (duplicateKeysTeamLeads.Count != 0)
        {
            Console.Error.WriteLine(
                $"Были найдены дублирующие ключи в team leads: {string.Join(", ", duplicateKeysTeamLeads)}");
            throw new InvalidOperationException(
                $"Были найдены дублирующие ключи в team leads: {string.Join(", ", duplicateKeysTeamLeads)}");
        }

        if (duplicateKeysJuniors.Count != 0)
        {
            Console.Error.WriteLine(
                $"Были найдены дублирующие ключи в juniors: {string.Join(", ", duplicateKeysJuniors)}");
            throw new InvalidOperationException(
                $"Были найдены дублирующие ключи в juniors: {string.Join(", ", duplicateKeysJuniors)}");
        }
    }


    private List<Team> GreedyStrategy(
        List<Preferences> juniors,
        List<Preferences> teamLeads,
        Dictionary<int, Dictionary<int, decimal>> compatibility)
    {
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();

        foreach (var junior in juniors)
        {
            var bestTeamLead = teamLeads
                .Where(tl => !assignedTeamLeads.Contains(tl.Member.Id))
                .OrderByDescending(tl => compatibility[junior.Member.Id][tl.Member.Id])
                .FirstOrDefault();

            if (bestTeamLead == null)
            {
                continue;
            }

            teams.Add(new Team(junior.Member, bestTeamLead.Member));
            assignedTeamLeads.Add(bestTeamLead.Member.Id);
        }

        return teams;
    }

    private List<Team> GlobalOptimizationStrategy(
        List<Preferences> juniors,
        List<Preferences> teamLeads,
        Dictionary<int, Dictionary<int, decimal>> compatibility)
    {
        var allPairs = juniors
            .SelectMany(junior => teamLeads.Select(lead => new
            {
                Junior = junior.Member,
                TeamLead = lead.Member,
                Compatibility = compatibility[junior.Member.Id][lead.Member.Id]
            }))
            .OrderByDescending(pair => pair.Compatibility)
            .ToList();

        var teams = new List<Team>();
        var assignedJuniors = new HashSet<int>();
        var assignedTeamLeads = new HashSet<int>();

        foreach (var pair in allPairs)
        {
            if (!assignedJuniors.Contains(pair.Junior.Id) && !assignedTeamLeads.Contains(pair.TeamLead.Id))
            {
                teams.Add(new Team(pair.Junior, pair.TeamLead));
                assignedJuniors.Add(pair.Junior.Id);
                assignedTeamLeads.Add(pair.TeamLead.Id);
            }
        }

        return teams;
    }

    private List<Team> StochasticStrategy(
        List<Preferences> juniors,
        List<Preferences> teamLeads,
        Dictionary<int, Dictionary<int, decimal>> compatibility,
        int iterations,
        List<Preferences> allPreferences)
    {
        var random = new Random();
        var bestTeams = new List<Team>();
        decimal bestHarmony = decimal.MinValue;

        for (var i = 0; i < iterations; i++)
        {
            var shuffledTeamLeads = teamLeads.OrderBy(_ => random.Next()).ToList();
            var teams = GreedyStrategy(juniors, shuffledTeamLeads, compatibility);
            var harmony = CalculateHarmony(teams, allPreferences);

            if (harmony > bestHarmony)
            {
                bestHarmony = harmony;
                bestTeams = teams;
            }
        }

        return bestTeams;
    }

    private List<Team> HungarianAlgorithmStrategy(
        List<Preferences> juniorPreferences,
        List<Preferences> teamLeadsPreferences,
        Dictionary<int, Dictionary<int, decimal>> compatibility)
    {
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();
        var assignedJuniors = new HashSet<int>();

        var allPairs = compatibility
            .SelectMany(junior => junior.Value,
                (junior, teamLead) => new
                {
                    JuniorId = junior.Key,
                    TeamLeadId = teamLead.Key,
                    Metric = teamLead.Value
                })
            .OrderByDescending(pair => pair.Metric)
            .ToList();

        foreach (var pair in allPairs)
        {
            if (assignedJuniors.Contains(pair.JuniorId) || assignedTeamLeads.Contains(pair.TeamLeadId))
            {
                continue;
            }

            var junior = juniorPreferences.First(p => p.Member.Id == pair.JuniorId);
            var teamLead = teamLeadsPreferences.First(p => p.Member.Id == pair.TeamLeadId);

            teams.Add(new Team(junior.Member, teamLead.Member));

            assignedJuniors.Add(pair.JuniorId);
            assignedTeamLeads.Add(pair.TeamLeadId);
        }

        return teams;
    }

    private static decimal CalculateHarmony(List<Team> teams, List<Preferences> preferences)
    {
        if (teams == null || teams.Count == 0)
        {
            Console.Error.WriteLine("Список команд пуст или null.");
            return 0m;
        }

        if (preferences == null || preferences.Count == 0)
        {
            Console.Error.WriteLine("Список предпочтений пуст или null.");
            throw new ArgumentException("Список предпочтений не должен быть пустым.");
        }

        var max = teams.Count * 2;

        // Сборка всех оценок (индексов удовлетворенности)
        var allHackathonMembersRates = teams.SelectMany(team =>
        {
            // Найти предпочтения джуниора
            var junPrefs = preferences.FirstOrDefault(it =>
                it.Member.EmployeeType == EmployeeType.Junior && it.Member.Id == team.Junior.Id);
            if (junPrefs == null)
            {
                Console.Error.WriteLine($"Предпочтения для junior с ID {team.Junior.Id} не найдены.");
                throw new InvalidOperationException($"Предпочтения для junior с ID {team.Junior.Id} не найдены.");
            }

            // Найти предпочтения тимлида
            var teamLeadPrefs = preferences.FirstOrDefault(it =>
                it.Member.EmployeeType == EmployeeType.TeamLead && it.Member.Id == team.TeamLead.Id);
            if (teamLeadPrefs == null)
            {
                Console.Error.WriteLine($"Предпочтения для team lead с ID {team.TeamLead.Id} не найдены.");
                throw new InvalidOperationException($"Предпочтения для team lead с ID {team.TeamLead.Id} не найдены.");
            }

            // Рассчитать индексы удовлетворенности
            var junToTeamLeadRate = junPrefs.PreferencesList.Contains(team.TeamLead.Id)
                ? max - junPrefs.PreferencesList.IndexOf(team.TeamLead.Id)
                : max;
            var teamLeadToJunRate = teamLeadPrefs.PreferencesList.Contains(team.Junior.Id)
                ? max - teamLeadPrefs.PreferencesList.IndexOf(team.Junior.Id)
                : max;

            Console.WriteLine(
                $"Индексы удовлетворённости для джуниора ({team.Junior.Id}) и тимлида ({team.TeamLead.Id}): {junToTeamLeadRate}, {teamLeadToJunRate}");

            return new[] { junToTeamLeadRate, teamLeadToJunRate };
        }).ToList();

        // Вычисление гармонической меры
        return HarmonyImpl(allHackathonMembersRates);
    }

    private static decimal HarmonyImpl(List<int> seq)
    {
        if (seq == null || seq.Count == 0)
        {
            Console.Error.WriteLine("Список оценок пуст или null.");
            return 0m;
        }

        var acc = seq.Sum(x => 1m / x);
        return seq.Count / acc;
    }


    private static Dictionary<int, Dictionary<int, decimal>> CalculateCompatibilityMatrix(
        List<Preferences> juniors,
        List<Preferences> teamLeads)
    {
        return juniors.ToDictionary(
            junior => junior.Member.Id,
            junior => teamLeads.ToDictionary(
                teamLead => teamLead.Member.Id,
                teamLead => CalculateEnjoyableMetric(junior, teamLead)
            ));
    }

    private static decimal CalculateEnjoyableMetric(Preferences juniorPreferences, Preferences teamLeadPreferences)
    {
        var juniorPrefs = juniorPreferences.PreferencesList;
        var teamLeadPrefs = teamLeadPreferences.PreferencesList;
        var max = teamLeadPreferences.PreferencesList.Count;

        var juniorToLead = juniorPrefs.Contains(teamLeadPreferences.Member.Id)
            ? max - juniorPrefs.IndexOf(teamLeadPreferences.Member.Id)
            : 0;

        var leadToJunior = teamLeadPrefs.Contains(juniorPreferences.Member.Id)
            ? max - teamLeadPrefs.IndexOf(juniorPreferences.Member.Id)
            : 0;

        return juniorToLead > 0 && leadToJunior > 0
            ? 1m / juniorToLead + 1m / leadToJunior
            : decimal.MinValue;
    }

    private static List<Team> FormTeams(
        List<Preferences> juniorPreferences,
        List<Preferences> teamLeadsPreferences,
        Dictionary<int, Dictionary<int, decimal>> compatibility)
    {
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();

        foreach (var junior in juniorPreferences)
        {
            var bestTeamLead = teamLeadsPreferences
                .Where(tl => !assignedTeamLeads.Contains(tl.Member.Id))
                .OrderByDescending(tl => compatibility[junior.Member.Id][tl.Member.Id])
                .FirstOrDefault();

            if (bestTeamLead == null)
            {
                continue;
            }

            teams.Add(new Team(junior.Member, bestTeamLead.Member));
            assignedTeamLeads.Add(bestTeamLead.Member.Id);
        }

        return teams;
    }

    private static List<Team> OptimizeTeamFormation(
        List<Preferences> juniorPreferences,
        List<Preferences> teamLeadsPreferences,
        Dictionary<int, Dictionary<int, decimal>> compatibility)
    {
        var teams = new List<Team>();
        var assignedTeamLeads = new HashSet<int>();
        var assignedJuniors = new HashSet<int>();

        var allPairs = compatibility
            .SelectMany(junior => junior.Value,
                (junior, teamLead) => new
                {
                    JuniorId = junior.Key,
                    TeamLeadId = teamLead.Key,
                    Metric = teamLead.Value
                })
            .OrderByDescending(pair => pair.Metric)
            .ToList();

        foreach (var pair in allPairs)
        {
            if (assignedJuniors.Contains(pair.JuniorId) || assignedTeamLeads.Contains(pair.TeamLeadId))
            {
                continue;
            }

            var junior = juniorPreferences.First(p => p.Member.Id == pair.JuniorId);
            var teamLead = teamLeadsPreferences.First(p => p.Member.Id == pair.TeamLeadId);

            teams.Add(new Team(junior.Member, teamLead.Member));

            assignedJuniors.Add(pair.JuniorId);
            assignedTeamLeads.Add(pair.TeamLeadId);
        }

        return teams;
    }
}