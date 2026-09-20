using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Things a passive item or totem blessing can change. The first five keep their original order
    /// (saved assets refer to the numbers): new stats are only ever appended.
    /// </summary>
    public enum StatType
    {
        /// <summary>Percent of base max HP (0.2 = +20%).</summary>
        MaxHp,
        /// <summary>Percent (0.1 = +10%).</summary>
        MoveSpeed,
        /// <summary>Percent (0.1 = +10%).</summary>
        Damage,
        /// <summary>Percent (0.25 = +25%).</summary>
        PickupRadius,
        /// <summary>Percent (0.1 = +10%).</summary>
        AttackSpeed,
        /// <summary>Flat HP (20 = +20 max HP).</summary>
        MaxHpFlat,
        /// <summary>HP per second.</summary>
        Regen,
        /// <summary>Flat damage removed from every hit taken.</summary>
        Armor,
        /// <summary>Fraction of damage dealt that heals you (0.02 = 2%).</summary>
        Lifesteal,
        /// <summary>Extra projectiles / blades / zones / pulses for weapons that have them.</summary>
        ExtraProjectiles,
        /// <summary>Added crit chance (0.05 = +5 points).</summary>
        CritChance,
        /// <summary>Added crit multiplier (0.2 = crits deal 2.2x instead of 2x).</summary>
        CritDamage,
        /// <summary>Percent of weapon area / range / radius.</summary>
        Area,
        /// <summary>Percent of projectile / boomerang / orbit speed.</summary>
        ProjectileSpeed,
        /// <summary>Extra enemies a projectile passes through.</summary>
        Pierce,
        /// <summary>Extra times a projectile jumps to another enemy.</summary>
        Bounces,
        /// <summary>Percent of lingering-zone duration.</summary>
        Duration,
        /// <summary>Percent of experience gained.</summary>
        XpGain,
        /// <summary>Chance to completely avoid a hit (0.04 = 4%).</summary>
        Dodge,
        /// <summary>Extra cards offered on level-up.</summary>
        ExtraChoices,
        /// <summary>Extra lives (revive at 50% HP).</summary>
        ExtraLives
    }

    /// <summary>Character base stats + traits + passive items + totem blessings = the values gameplay actually reads.</summary>
    public class PlayerStats : MonoBehaviour
    {
        private static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;

        private readonly List<BlessingData> _blessings = new List<BlessingData>();
        private readonly float[] _bonus = new float[StatCount];
        private IEnumerable<KeyValuePair<PassiveData, int>> _passives;
        private CharacterStats _base;

        public float MaxHp { get; private set; }
        public float MoveSpeed { get; private set; }
        public float DamageMultiplier { get; private set; }
        public float PickupRadius { get; private set; }
        /// <summary>1 = normal; 1.2 means weapons fire 20% faster.</summary>
        public float AttackSpeed { get; private set; }
        /// <summary>Multiplier on every weapon's area / range / radius.</summary>
        public float AreaMultiplier { get; private set; }
        public float CritChance { get; private set; }
        /// <summary>Damage multiplier of a critical hit (2 = double).</summary>
        public float CritMultiplier { get; private set; }
        public float XpMultiplier { get; private set; }
        public float RegenPerSecond { get; private set; }
        public float Armor { get; private set; }
        public float Lifesteal { get; private set; }
        public int ExtraAmount { get; private set; }
        public int ExtraPierce { get; private set; }
        public int ExtraBounces { get; private set; }
        public float ProjectileSpeedMultiplier { get; private set; }
        public float DurationMultiplier { get; private set; }
        public float DodgeChance { get; private set; }
        public int ExtraChoices { get; private set; }
        public int ExtraLives { get; private set; }
        public int BlessingCount => _blessings.Count;

        public event Action Changed;

        public void Init(CharacterStats baseStats)
        {
            _base = baseStats;
            _blessings.Clear();
            Recalculate(null);
        }

        /// <summary>Totem blessing: lasts until the end of the run (stats are rebuilt from the list).</summary>
        public void AddBlessing(BlessingData blessing)
        {
            _blessings.Add(blessing);
            Recalculate(_passives);
        }

        public void Recalculate(IEnumerable<KeyValuePair<PassiveData, int>> passives)
        {
            _passives = passives;
            Array.Clear(_bonus, 0, _bonus.Length);
            var power = _base.passivePower;

            if (passives != null)
            {
                foreach (var kv in passives)
                {
                    _bonus[(int)kv.Key.stat] += kv.Key.valuePerLevel * kv.Value * power;
                    if (kv.Key.hasSecondStat)
                        _bonus[(int)kv.Key.stat2] += kv.Key.valuePerLevel2 * kv.Value * power;
                }
            }

            foreach (var b in _blessings)
                _bonus[(int)b.stat] += b.value;

            MaxHp = Mathf.Max(10f, _base.maxHp * (1f + B(StatType.MaxHp)) + B(StatType.MaxHpFlat));
            MoveSpeed = Mathf.Max(1f, _base.moveSpeed * (1f + B(StatType.MoveSpeed)));
            DamageMultiplier = Mathf.Max(0.1f, _base.damageMultiplier * (1f + B(StatType.Damage)));
            PickupRadius = Mathf.Max(0.5f, _base.pickupRadius * (1f + B(StatType.PickupRadius)));
            AttackSpeed = Mathf.Max(0.3f, 1f + _base.attackSpeedBonus + B(StatType.AttackSpeed));
            AreaMultiplier = Mathf.Max(0.3f, 1f + _base.areaBonus + B(StatType.Area));
            CritChance = Mathf.Clamp01(_base.critChance + B(StatType.CritChance));
            CritMultiplier = 2f + B(StatType.CritDamage);
            XpMultiplier = _base.xpMultiplier * (1f + B(StatType.XpGain));
            RegenPerSecond = _base.regenPerSecond + B(StatType.Regen);
            Armor = _base.armor + B(StatType.Armor);
            Lifesteal = Mathf.Max(0f, B(StatType.Lifesteal));
            ExtraAmount = Mathf.RoundToInt(B(StatType.ExtraProjectiles));
            ExtraPierce = Mathf.RoundToInt(B(StatType.Pierce));
            ExtraBounces = Mathf.RoundToInt(B(StatType.Bounces));
            ProjectileSpeedMultiplier = Mathf.Max(0.3f, 1f + B(StatType.ProjectileSpeed));
            DurationMultiplier = Mathf.Max(0.3f, 1f + B(StatType.Duration));
            DodgeChance = Mathf.Clamp(B(StatType.Dodge), 0f, 0.6f);
            ExtraChoices = Mathf.RoundToInt(B(StatType.ExtraChoices));
            ExtraLives = Mathf.RoundToInt(B(StatType.ExtraLives));
            Changed?.Invoke();
        }

        private float B(StatType stat) => _bonus[(int)stat];
    }
}
