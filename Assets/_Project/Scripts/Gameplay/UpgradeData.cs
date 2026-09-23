using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Quality tier shown in the collection screen. Cosmetic only — does not affect gameplay balance.</summary>
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public static class RarityInfo
    {
        public static Color Color(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return new Color(0.4f, 0.7f, 1f, 1f);
                case Rarity.Epic: return new Color(0.72f, 0.45f, 1f, 1f);
                case Rarity.Legendary: return new Color(1f, 0.78f, 0.25f, 1f);
                default: return new Color(0.72f, 0.75f, 0.8f, 1f);
            }
        }

        /// <summary>English key for Loc.T — add translations to the localization files.</summary>
        public static string Key(Rarity r)
        {
            switch (r)
            {
                case Rarity.Rare: return "Rare";
                case Rarity.Epic: return "Epic";
                case Rarity.Legendary: return "Legendary";
                default: return "Common";
            }
        }
    }

    /// <summary>Base for everything offered on a level-up card (weapons and passive items).</summary>
    public abstract class UpgradeData : ScriptableObject
    {
        public string displayName = "Upgrade";
        [TextArea] public string description;
        public Sprite icon;
        [Tooltip("Cosmetic quality tier shown in the collection screen only.")]
        public Rarity rarity = Rarity.Common;

        public abstract int MaxLevel { get; }
        public abstract bool IsWeapon { get; }

        /// <summary>Owned items at their designed maximum keep growing (endless levels) when this is true.</summary>
        public virtual bool AllowsEndless => true;

        /// <summary>Text shown on the card when this upgrade is taken to <paramref name="level"/>.</summary>
        public abstract string GetCardText(int level);
    }
}
