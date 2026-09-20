using Rebonk.Core;
using System;
using System.Collections.Generic;

namespace Rebonk.Gameplay
{
    public enum QuestMetric
    {
        Kills,
        Zones,
        Bosses,
        Evolutions,
        Survival,
        Level,
        Runs
    }

    public struct QuestDef
    {
        public QuestMetric metric;
        public int target;

        public QuestDef(QuestMetric metric, int target)
        {
            this.metric = metric;
            this.target = target;
        }

        /// <summary>Survival and level are best-in-a-run goals; everything else adds up over the day.</summary>
        public bool IsBestOfRun => metric == QuestMetric.Survival || metric == QuestMetric.Level;

        public string Describe()
        {
            switch (metric)
            {
                case QuestMetric.Kills: return Loc.F("Kill {0} enemies", target);
                case QuestMetric.Zones: return Loc.F("Clear {0} zones", target);
                case QuestMetric.Bosses: return Loc.F("Defeat bosses: {0}", target);
                case QuestMetric.Evolutions: return Loc.F("Evolve weapons: {0}", target);
                case QuestMetric.Survival: return Loc.F("Survive against the spirits: {0}", Clock(target));
                case QuestMetric.Level: return Loc.F("Reach level {0} in one run", target);
                case QuestMetric.Runs: return Loc.F("Play {0} runs", target);
                default: return "";
            }
        }

        public string ProgressText(int progress)
        {
            var p = Math.Min(progress, target);
            return metric == QuestMetric.Survival ? Clock(p) + " / " + Clock(target) : p + " / " + target;
        }

        private static string Clock(int seconds) => (seconds / 60) + ":" + (seconds % 60).ToString("00");
    }

    /// <summary>
    /// Three quests a day, chosen from a fixed pool by the date, so they rotate at local midnight and are the
    /// same all day. Progress is folded in when a run ends. Each finished quest counts towards unlock conditions.
    /// </summary>
    public static class DailyQuests
    {
        public const int SlotCount = 3;

        private static readonly QuestDef[] Pool =
        {
            new QuestDef(QuestMetric.Kills, 150),
            new QuestDef(QuestMetric.Kills, 400),
            new QuestDef(QuestMetric.Kills, 800),
            new QuestDef(QuestMetric.Zones, 1),
            new QuestDef(QuestMetric.Zones, 3),
            new QuestDef(QuestMetric.Bosses, 1),
            new QuestDef(QuestMetric.Bosses, 2),
            new QuestDef(QuestMetric.Evolutions, 1),
            new QuestDef(QuestMetric.Survival, 90),
            new QuestDef(QuestMetric.Survival, 240),
            new QuestDef(QuestMetric.Level, 8),
            new QuestDef(QuestMetric.Level, 14),
            new QuestDef(QuestMetric.Runs, 3),
        };

        public static string Today => DateTime.Now.ToString("yyyyMMdd");

        public static TimeSpan TimeUntilReset => DateTime.Today.AddDays(1) - DateTime.Now;

        /// <summary>Starts a fresh set of quests if the saved one is from another day.</summary>
        public static void EnsureToday(SaveData d)
        {
            var today = Today;
            if (d.dailyDate == today && d.dailyPick.Count == SlotCount && d.dailyProgress.Count == SlotCount
                && d.dailyDone.Count == SlotCount && ValidPicks(d))
                return;

            var rng = new Random(int.Parse(today));
            var indices = new List<int>();
            for (var i = 0; i < Pool.Length; i++)
                indices.Add(i);
            for (var i = indices.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                var tmp = indices[i];
                indices[i] = indices[j];
                indices[j] = tmp;
            }

            d.dailyDate = today;
            d.dailyPick.Clear();
            d.dailyProgress.Clear();
            d.dailyDone.Clear();
            var used = new HashSet<QuestMetric>();
            foreach (var i in indices)
            {
                if (d.dailyPick.Count == SlotCount)
                    break;
                if (!used.Add(Pool[i].metric))
                    continue;
                d.dailyPick.Add(i);
                d.dailyProgress.Add(0);
                d.dailyDone.Add(false);
            }
        }

        public static QuestDef Get(SaveData d, int slot) => Pool[d.dailyPick[slot]];

        /// <summary>Adds a finished run to today's quests. Returns the descriptions of quests completed just now.</summary>
        public static List<string> Apply(SaveData d, RunResult run)
        {
            EnsureToday(d);
            var completed = new List<string>();
            for (var i = 0; i < SlotCount; i++)
            {
                var quest = Get(d, i);
                var gain = Gain(quest.metric, run);
                d.dailyProgress[i] = quest.IsBestOfRun ? Math.Max(d.dailyProgress[i], gain) : d.dailyProgress[i] + gain;

                if (!d.dailyDone[i] && d.dailyProgress[i] >= quest.target)
                {
                    d.dailyDone[i] = true;
                    d.questsCompleted++;
                    completed.Add(quest.Describe());
                }
            }
            return completed;
        }

        private static int Gain(QuestMetric metric, RunResult run)
        {
            switch (metric)
            {
                case QuestMetric.Kills: return run.Kills;
                case QuestMetric.Zones: return run.ZonesCleared;
                case QuestMetric.Bosses: return run.Bosses;
                case QuestMetric.Evolutions: return run.Evolutions;
                case QuestMetric.Survival: return (int)run.SurvivalSeconds;
                case QuestMetric.Level: return run.Level;
                case QuestMetric.Runs: return 1;
                default: return 0;
            }
        }

        private static bool ValidPicks(SaveData d)
        {
            foreach (var i in d.dailyPick)
                if (i < 0 || i >= Pool.Length)
                    return false;
            return true;
        }
    }
}
