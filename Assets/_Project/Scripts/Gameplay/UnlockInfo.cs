using Rebonk.Core;
using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public enum UnlockType
    {
        TotalKills,
        TotalRuns,
        BestKillsInRun,
        BestZonesCleared,
        BestSurvivalSeconds,
        BossDefeated,
        BestLevelInRun,
        TotalEvolutions,
        TotalBossKills,
        TotalZonesCleared,
        TotalPlayMinutes,
        BestRunMinutes,
        DailyQuestsCompleted,
        AchievementsEarned
    }

    /// <summary>Unlock rule shared by characters and weapons. Data only: new rules need no code.</summary>
    [Serializable]
    public class UnlockInfo
    {
        [Tooltip("Stable save id. Never change it after release.")]
        public string id;
        public bool unlockedByDefault = true;
        public UnlockType type = UnlockType.TotalKills;
        public float target = 100f;
        [Tooltip("Only for BossDefeated.")]
        public EnemyData boss;

        public bool IsUnlocked => unlockedByDefault || SaveSystem.Data.unlockedIds.Contains(id);

        public float Current(SaveData d)
        {
            switch (type)
            {
                case UnlockType.TotalKills: return d.totalKills;
                case UnlockType.TotalRuns: return d.totalRuns;
                case UnlockType.BestKillsInRun: return d.bestKills;
                case UnlockType.BestZonesCleared: return d.bestZones;
                case UnlockType.BestSurvivalSeconds: return d.bestSurvival;
                case UnlockType.BossDefeated: return boss != null ? d.GetBossKills(boss.name) : 0;
                case UnlockType.BestLevelInRun: return d.bestLevel;
                case UnlockType.TotalEvolutions: return d.totalEvolutions;
                case UnlockType.TotalBossKills: return d.TotalBossKills;
                case UnlockType.TotalZonesCleared: return d.totalZones;
                case UnlockType.TotalPlayMinutes: return d.totalPlaySeconds / 60f;
                case UnlockType.BestRunMinutes: return d.bestRunSeconds / 60f;
                case UnlockType.DailyQuestsCompleted: return d.questsCompleted;
                case UnlockType.AchievementsEarned: return d.achievementIds.Count;
                default: return 0;
            }
        }

        public bool IsMet(SaveData d) => type == UnlockType.BossDefeated ? Current(d) >= 1f : Current(d) >= target;

        public string Describe()
        {
            var t = (int)target;
            switch (type)
            {
                case UnlockType.TotalKills: return Loc.F("Enemies killed in total: {0}", t);
                case UnlockType.TotalRuns: return Loc.F("Runs played: {0}", t);
                case UnlockType.BestKillsInRun: return Loc.F("Enemies killed in one run: {0}", t);
                case UnlockType.BestZonesCleared: return Loc.F("Zones cleared in one run: {0}", t);
                case UnlockType.BestSurvivalSeconds: return Loc.F("Survive against the spirits: {0}", Clock(t));
                case UnlockType.BossDefeated: return Loc.F("Defeat: {0}", boss != null ? Loc.T(boss.displayName) : Loc.T("the boss"));
                case UnlockType.BestLevelInRun: return Loc.F("Level reached in one run: {0}", t);
                case UnlockType.TotalEvolutions: return Loc.F("Weapon evolutions: {0}", t);
                case UnlockType.TotalBossKills: return Loc.F("Bosses defeated in total: {0}", t);
                case UnlockType.TotalZonesCleared: return Loc.F("Zones cleared in total: {0}", t);
                case UnlockType.TotalPlayMinutes: return Loc.F("Minutes played in total: {0}", t);
                case UnlockType.BestRunMinutes: return Loc.F("Minutes survived in one run: {0}", t);
                case UnlockType.DailyQuestsCompleted: return Loc.F("Daily quests completed: {0}", t);
                case UnlockType.AchievementsEarned: return Loc.F("Achievements earned: {0}", t);
                default: return "";
            }
        }

        public string ProgressText(SaveData d)
        {
            if (type == UnlockType.BossDefeated)
                return "";
            if (type == UnlockType.BestSurvivalSeconds)
                return Clock((int)Current(d)) + " / " + Clock((int)target);
            return (int)Current(d) + " / " + (int)target;
        }

        private static string Clock(int seconds) => (seconds / 60) + ":" + (seconds % 60).ToString("00");
    }
}
