using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Base for everything offered on a level-up card (weapons and passive items).</summary>
    public abstract class UpgradeData : ScriptableObject
    {
        public string displayName = "Upgrade";
        [TextArea] public string description;
        public Sprite icon;

        public abstract int MaxLevel { get; }
        public abstract bool IsWeapon { get; }

        /// <summary>Text shown on the card when this upgrade is taken to <paramref name="level"/>.</summary>
        public abstract string GetCardText(int level);
    }
}
