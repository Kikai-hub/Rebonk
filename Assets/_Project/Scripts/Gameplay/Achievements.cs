using System.Collections.Generic;

namespace Rebonk.Gameplay
{
    public class AchievementDef
    {
        public string id;
        /// <summary>English display name; also the localization key.</summary>
        public string name;
        public UnlockInfo rule;
    }

    /// <summary>
    /// Long-term goals. Each is an unlock rule checked against the saved totals, so progress needs no extra
    /// storage: only the ids of the earned ones are saved. Earned once, kept for good.
    /// </summary>
    public static class Achievements
    {
        public static readonly List<AchievementDef> All = new List<AchievementDef>
        {
            Make("kills_100", "Rookie Hunter", UnlockType.TotalKills, 100),
            Make("kills_1000", "Monster Slayer", UnlockType.TotalKills, 1000),
            Make("kills_10000", "Legion Breaker", UnlockType.TotalKills, 10000),
            Make("runs_1", "First Steps", UnlockType.TotalRuns, 1),
            Make("runs_10", "Regular", UnlockType.TotalRuns, 10),
            Make("runs_50", "Veteran", UnlockType.TotalRuns, 50),
            Make("run_kills_300", "Massacre", UnlockType.BestKillsInRun, 300),
            Make("run_kills_1000", "Bloodbath", UnlockType.BestKillsInRun, 1000),
            Make("run_zones_2", "Zone Hopper", UnlockType.BestZonesCleared, 2),
            Make("run_zones_4", "Marathoner", UnlockType.BestZonesCleared, 4),
            Make("survive_180", "Hold On", UnlockType.BestSurvivalSeconds, 180),
            Make("survive_600", "Endless Night", UnlockType.BestSurvivalSeconds, 600),
            Make("boss_1", "Giant Killer", UnlockType.TotalBossKills, 1),
            Make("boss_10", "Boss Hunter", UnlockType.TotalBossKills, 10),
            Make("level_10", "Growing Strong", UnlockType.BestLevelInRun, 10),
            Make("level_20", "Power Spike", UnlockType.BestLevelInRun, 20),
            Make("evolve_1", "Evolved", UnlockType.TotalEvolutions, 1),
            Make("evolve_10", "Master of Arms", UnlockType.TotalEvolutions, 10),
            Make("long_run_15", "Long Haul", UnlockType.BestRunMinutes, 15),
            Make("playtime_120", "Dedicated", UnlockType.TotalPlayMinutes, 120),
            Make("daily_5", "Daily Habit", UnlockType.DailyQuestsCompleted, 5),
            Make("daily_30", "Quest Master", UnlockType.DailyQuestsCompleted, 30),
        };

        public static bool IsEarned(SaveData d, AchievementDef a) => d.achievementIds.Contains(a.id);

        public static int EarnedCount(SaveData d) => d.achievementIds.Count;

        /// <summary>Earns everything whose condition is now met. Returns the English names of the new ones.</summary>
        public static List<string> Evaluate(SaveData d)
        {
            var result = new List<string>();
            foreach (var a in All)
            {
                if (IsEarned(d, a) || !a.rule.IsMet(d))
                    continue;
                d.achievementIds.Add(a.id);
                result.Add(a.name);
            }
            return result;
        }

        private static AchievementDef Make(string id, string name, UnlockType type, float target)
        {
            return new AchievementDef
            {
                id = id,
                name = name,
                rule = new UnlockInfo { id = id, unlockedByDefault = false, type = type, target = target }
            };
        }
    }
}
