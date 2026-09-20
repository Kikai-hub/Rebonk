using System;
using UnityEngine;

namespace Rebonk.Core
{
    public class Health : MonoBehaviour
    {
        public float Max { get; private set; }
        public float Current { get; private set; }
        public bool IsDead => Current <= 0f;

        /// <summary>Flat damage removed from every hit (at least 1 damage always goes through). Used for armor.</summary>
        public float FlatReduction;
        /// <summary>Chance (0..1) to ignore a hit completely.</summary>
        public float DodgeChance;
        /// <summary>While true no damage is taken (short grace after a revive).</summary>
        public bool Invulnerable;
        /// <summary>Called when HP reaches zero; return true to cancel the death (revive).</summary>
        public Func<Health, bool> BeforeDeath;

        /// <summary>Fired with (current, max) on init and on every change.</summary>
        public event Action<float, float> Changed;
        public event Action<Health> Died;

        public void Init(float max)
        {
            Max = max;
            Current = max;
            Changed?.Invoke(Current, Max);
        }

        /// <summary>Changes max HP; when it grows, current HP grows by the same amount.</summary>
        public void SetMax(float max)
        {
            var delta = max - Max;
            Max = max;
            Current = Mathf.Clamp(Current + Mathf.Max(0f, delta), 0f, Max);
            Changed?.Invoke(Current, Max);
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            Current = Mathf.Min(Max, Current + amount);
            Changed?.Invoke(Current, Max);
        }

        /// <summary>Sets HP to a fraction of max (revive).</summary>
        public void Restore(float fraction)
        {
            Current = Mathf.Clamp(Max * fraction, 1f, Max);
            Changed?.Invoke(Current, Max);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f || Invulnerable)
                return;

            if (DodgeChance > 0f && UnityEngine.Random.value < DodgeChance)
                return;

            if (FlatReduction > 0f)
                amount = Mathf.Max(1f, amount - FlatReduction);

            Current = Mathf.Max(0f, Current - amount);
            Changed?.Invoke(Current, Max);

            if (Current > 0f)
                return;

            if (BeforeDeath != null && BeforeDeath(this))
                return;

            Died?.Invoke(this);
        }
    }
}
