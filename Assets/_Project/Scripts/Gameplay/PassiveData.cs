using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// A passive "book" item. Adds <c>valuePerLevel</c> of a stat per level; optionally a second stat
    /// (usually a trade-off, e.g. +damage but -max HP). Units per stat: see <see cref="StatType"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Rebonk/Passive Item", fileName = "Passive_")]
    public class PassiveData : UpgradeData
    {
        public StatType stat;
        [Tooltip("Bonus per level, in the units of the stat (percent stats: 0.1 = +10%).")]
        public float valuePerLevel = 0.1f;
        public bool hasSecondStat;
        public StatType stat2;
        [Tooltip("Second bonus per level (negative for a trade-off).")]
        public float valuePerLevel2;
        public int maxLevel = 5;

        public override int MaxLevel => maxLevel;
        public override bool IsWeapon => false;

        /// <summary>Only ordinary stat books grow forever; special ones (extra lives, extra cards, ...) stay capped.</summary>
        public override bool AllowsEndless => maxLevel >= 5;

        public override string GetCardText(int level) => description;
    }
}
