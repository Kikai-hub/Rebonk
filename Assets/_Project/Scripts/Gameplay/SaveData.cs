using System;
using System.Collections.Generic;

namespace Rebonk.Gameplay
{
    [Serializable]
    public class BossKill
    {
        public string id;
        public int count;
    }

    /// <summary>Everything persisted between launches (meta progression only; nothing from inside a run).</summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;

        public List<string> unlockedIds = new List<string>();
        public string selectedCharacterId;

        public int totalKills;
        public int totalRuns;
        public int bestKills;
        public int bestZones;
        public float bestSurvival;
        public int bestLevel;
        public int totalEvolutions;
        public int totalZones;
        public float totalPlaySeconds;
        public float bestRunSeconds;
        public List<BossKill> bossKills = new List<BossKill>();

        public List<string> achievementIds = new List<string>();
        public int questsCompleted;
        public string dailyDate;
        public List<int> dailyPick = new List<int>();
        public List<int> dailyProgress = new List<int>();
        public List<bool> dailyDone = new List<bool>();

        public int TotalBossKills
        {
            get
            {
                var n = 0;
                foreach (var b in bossKills)
                    n += b.count;
                return n;
            }
        }

        public int GetBossKills(string bossId)
        {
            foreach (var b in bossKills)
                if (b.id == bossId)
                    return b.count;
            return 0;
        }

        public void AddBossKill(string bossId)
        {
            foreach (var b in bossKills)
            {
                if (b.id == bossId)
                {
                    b.count++;
                    return;
                }
            }
            bossKills.Add(new BossKill { id = bossId, count = 1 });
        }
    }
}
