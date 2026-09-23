using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Base for all weapon behaviours. Created by <see cref="Inventory"/>, driven by <see cref="WeaponData"/> level tables.</summary>
    public abstract class Weapon : MonoBehaviour
    {
        protected PlayerController Player { get; private set; }
        protected WeaponLevelStats Current { get; private set; }

        public WeaponData Data { get; private set; }
        public int Level { get; private set; }

        /// <summary>Levels taken beyond the designed maximum (endless upgrades).</summary>
        public int OverLevel => Data != null ? Mathf.Max(0, Level - Data.MaxLevel) : 0;

        /// <summary>Each endless level adds this much damage (0.15 = +15%) and a little area.</summary>
        public const float EndlessDamagePerLevel = 0.15f;
        public const float EndlessAreaPerLevel = 0.02f;
        private const float EndlessAreaCap = 0.5f;

        /// <summary>Damage for one attack: level table x character multiplier, multiplied on a critical hit.</summary>
        protected float Damage
        {
            get
            {
                var d = Current.damage * Player.Stats.DamageMultiplier * (1f + EndlessDamagePerLevel * OverLevel);
                var crit = Player.Stats.CritChance;
                if (crit > 0f && Random.value < crit)
                    d *= Player.Stats.CritMultiplier;
                return d;
            }
        }

        protected float Cooldown => Current.cooldown / Player.Stats.AttackSpeed;

        /// <summary>The level table's area / range / radius, scaled by the area bonus.</summary>
        protected float Area => Current.area * Player.Stats.AreaMultiplier * (1f + Mathf.Min(EndlessAreaCap, EndlessAreaPerLevel * OverLevel));

        /// <summary>Projectiles / blades / zones / pulses per use, including items and blessings that add more.</summary>
        protected int Amount => Mathf.Max(1, Current.amount + Player.Stats.ExtraAmount);

        protected int Pierce => Current.pierce + Player.Stats.ExtraPierce;

        protected int Bounces => Current.bounces + Player.Stats.ExtraBounces;

        /// <summary>Flight / rotation speed of projectiles, boomerangs and orbiting blades.</summary>
        protected float Speed => Current.speed * Player.Stats.ProjectileSpeedMultiplier;

        /// <summary>Lifetime of lingering zones.</summary>
        protected float Duration => Current.duration * Player.Stats.DurationMultiplier;

        protected bool Ready => Data != null && !Player.Health.IsDead;

        public void Init(WeaponData data, PlayerController player)
        {
            Data = data;
            Player = player;
            SetLevel(1);
        }

        public void SetLevel(int level)
        {
            Level = Mathf.Max(1, level); // above MaxLevel the last table row is reused plus the endless bonus
            Current = Data.GetLevel(Level);
            OnLevelChanged();
        }

        protected virtual void OnLevelChanged() { }
    }
}
