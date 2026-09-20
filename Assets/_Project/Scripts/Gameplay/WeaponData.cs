using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Numbers for one weapon level. Meaning of the generic fields depends on the weapon type:
    /// melee: area = range | projectile: speed = flight speed, amount = projectiles per shot, pierce = enemies passed through |
    /// orbit: area = orbit radius, speed = degrees/s, amount = blades, cooldown = damage tick |
    /// area (nova): area = radius | area (zones): area = zone radius, speed = tick interval, duration = zone lifetime, amount = zones per cast |
    /// boomerang: area = max distance out, speed = flight speed, amount = boomerangs per throw.
    /// </summary>
    [Serializable]
    public class WeaponLevelStats
    {
        public float damage = 10f;
        public float cooldown = 1f;
        public float area = 2f;
        public float speed = 10f;
        public int amount = 1;
        public int pierce = 1;
        public float duration = 2f;
        [Tooltip("Projectiles: how many times a projectile jumps to another enemy after a hit.")]
        public int bounces;
        [Tooltip("Card text when this level is taken (level 1 uses the description).")]
        public string upgradeText;
    }

    public enum WeaponCategory
    {
        /// <summary>Short reach: swings and stabs in front of (or right around) the player.</summary>
        Near,
        /// <summary>Shoots or throws at enemies far away.</summary>
        Ranged,
        /// <summary>Works in a radius around the player: auras, orbits, blasts, drops.</summary>
        Radius
    }

    [CreateAssetMenu(menuName = "Rebonk/Weapon", fileName = "Weapon_")]
    public class WeaponData : UpgradeData
    {
        [Tooltip("Only used to group weapons in the collection screen.")]
        public WeaponCategory category;
        public Weapon weaponPrefab;
        [Tooltip("Locked weapons never show up as NEW level-up cards until unlocked (a character may still start with one).")]
        public UnlockInfo unlock = new UnlockInfo { unlockedByDefault = true };
        public WeaponLevelStats[] levels = new WeaponLevelStats[1] { new WeaponLevelStats() };

        public override int MaxLevel => levels.Length;
        public override bool IsWeapon => true;

        public WeaponLevelStats GetLevel(int level) => levels[Mathf.Clamp(level, 1, levels.Length) - 1];

        public override string GetCardText(int level) =>
            level <= 1 ? description : GetLevel(level).upgradeText;
    }
}
