using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    [CreateAssetMenu(menuName = "Rebonk/Character Stats", fileName = "Character_")]
    public class CharacterStats : ScriptableObject
    {
        public string displayName = "Hero";
        [TextArea] public string description;
        [Tooltip("Short name of the character's unique trait, shown in the menu.")]
        public string traitName;
        [Tooltip("In-game body sprite and menu portrait (16x16).")]
        public Sprite sprite;
        public UnlockInfo unlock = new UnlockInfo { unlockedByDefault = true };

        [Header("Base stats")]
        public float maxHp = 100f;
        public float moveSpeed = 4f;
        [Tooltip("Multiplier applied to every weapon's base damage.")]
        public float damageMultiplier = 1f;
        public float pickupRadius = 2f;

        [Header("Starting weapons")]
        public WeaponData startingWeapon;
        [Tooltip("Additional weapons the character starts with.")]
        public List<WeaponData> extraStartingWeapons = new List<WeaponData>();
        [Tooltip("Ignore startingWeapon and start every run with a random unlocked weapon.")]
        public bool randomStartWeapon;

        [Header("Unique traits (0 / 1 = neutral)")]
        [Tooltip("Added to the base attack speed (0.15 = +15%).")]
        public float attackSpeedBonus;
        [Tooltip("Added to the size of every weapon's area/range (0.25 = +25%).")]
        public float areaBonus;
        public float regenPerSecond;
        [Tooltip("Flat damage removed from every hit taken.")]
        public float armor;
        [Range(0f, 1f)] public float critChance;
        public float xpMultiplier = 1f;
        [Tooltip("Multiplier on the bonus of every passive item.")]
        public float passivePower = 1f;
        [Tooltip("Cards offered on level-up.")]
        [Range(3, 4)] public int levelUpChoices = 3;
        [Tooltip("Spirit contact/bullet damage multiplier (0.7 = 30% less).")]
        public float spiritDamageMultiplier = 1f;
        [Tooltip("Multiplier on how fast spirits grow stronger per second (0.7 = slower).")]
        public float spiritGrowthMultiplier = 1f;
        [Tooltip("Extra lives per run (revive at 50% HP).")]
        public int revives;
    }
}
