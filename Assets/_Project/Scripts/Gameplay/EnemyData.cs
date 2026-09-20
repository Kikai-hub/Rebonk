using UnityEngine;

namespace Rebonk.Gameplay
{
    public enum EnemyBehavior
    {
        /// <summary>Walks straight at the player (basic, tank, swarmer archetypes differ only by stats).</summary>
        Chase,
        /// <summary>Keeps its distance and shoots projectiles at the player.</summary>
        Ranged,
        /// <summary>Walks, then telegraphs and dashes in a straight line.</summary>
        Charge,
        /// <summary>Moves and attacks under the control of a <see cref="BossController"/>.</summary>
        Boss
    }

    [CreateAssetMenu(menuName = "Rebonk/Enemy Data", fileName = "Enemy_")]
    public class EnemyData : ScriptableObject
    {
        public string displayName = "Enemy";
        public EnemyBehavior behavior = EnemyBehavior.Chase;

        [Header("Core stats (before time / zone scaling)")]
        public float maxHp = 20f;
        public float moveSpeed = 1.6f;
        public float contactDamage = 5f;
        [Tooltip("Distance to the player at which contact damage applies.")]
        public float contactRadius = 0.6f;
        public float contactCooldown = 0.6f;
        [Tooltip("Experience granted (as a gem) when this enemy dies.")]
        public int xpValue = 1;
        [Tooltip("Spirits are the survival-phase enemies: their speed/damage grow every second of that phase.")]
        public bool isSpirit;

        [Header("Ranged behaviour")]
        [Tooltip("Tries to stay about this far from the player.")]
        public float preferredDistance = 6f;
        public float shootInterval = 2.2f;
        public float projectileSpeed = 5f;
        public float projectileDamage = 6f;
        public float projectileLifetime = 3f;

        [Header("Charge behaviour")]
        public float chargeTriggerRange = 7f;
        [Tooltip("Standing still and glowing red before the dash (the player's chance to dodge).")]
        public float chargeWindup = 0.8f;
        public float chargeSpeed = 9f;
        public float chargeDuration = 0.6f;
        public float chargeCooldown = 3f;
    }
}
