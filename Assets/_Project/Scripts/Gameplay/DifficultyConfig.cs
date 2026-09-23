using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Multipliers applied to an enemy at spawn time.</summary>
    public struct EnemyScale
    {
        public float Hp;
        public float Speed;
        public float Damage;

        public static EnemyScale One => new EnemyScale { Hp = 1f, Speed = 1f, Damage = 1f };
    }

    /// <summary>
    /// Every difficulty coefficient in one asset, for balancing without touching code.
    /// Stats:  multiplier = (1 + rate * minutes + quadratic * minutes^2) * (1 + perZone) ^ (zoneLevel - 1).
    /// Spawns: interval shrinks linearly to a floor; batch size and alive cap grow per minute.
    /// </summary>
    [CreateAssetMenu(menuName = "Rebonk/Difficulty Config", fileName = "DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Enemy stats per minute of run time (linear)")]
        public float hpPerMinute = 0.20f;
        public float damagePerMinute = 0.10f;
        public float speedPerMinute = 0.03f;
        [Tooltip("Extra growth that speeds up over time: adds this x minutes^2 to the multiplier, so long runs stay dangerous however strong the player is.")]
        public float hpQuadraticPerMinute = 0.015f;
        public float damageQuadraticPerMinute = 0.008f;
        [Tooltip("Speed scaling stops here so late enemies cannot outrun the player.")]
        public float maxSpeedMultiplier = 1.8f;

        [Header("Extra scaling per zone after the first (compounds: 0.4 = x1.4 per zone)")]
        public float perZone = 0.4f;

        [Header("Spawn rate")]
        public float startSpawnInterval = 0.5f;
        public float minSpawnInterval = 0.12f;
        [Tooltip("Run minutes over which the interval goes from start to min.")]
        public float spawnRampMinutes = 10f;
        [Tooltip("Spawn events at once = 1 + floor(minutes * this).")]
        public float extraPerSpawnPerMinute = 0.35f;

        [Header("Population cap")]
        public int baseMaxAlive = 120;
        public int maxAliveGrowthPerMinute = 25;
        public int hardMaxAlive = 500;

        public EnemyScale EvaluateScale(float minutes, int zoneLevel)
        {
            var zone = Mathf.Pow(1f + perZone, Mathf.Max(0, zoneLevel - 1));
            return new EnemyScale
            {
                Hp = (1f + hpPerMinute * minutes + hpQuadraticPerMinute * minutes * minutes) * zone,
                Damage = (1f + damagePerMinute * minutes + damageQuadraticPerMinute * minutes * minutes) * zone,
                Speed = Mathf.Min(1f + speedPerMinute * minutes, maxSpeedMultiplier)
            };
        }

        public float SpawnInterval(float minutes) =>
            Mathf.Max(0.05f, Mathf.Lerp(startSpawnInterval, minSpawnInterval, Mathf.Clamp01(minutes / Mathf.Max(0.01f, spawnRampMinutes))));

        public int SpawnBatch(float minutes) => 1 + Mathf.FloorToInt(minutes * extraPerSpawnPerMinute);

        public int MaxAlive(float minutes) =>
            Mathf.Min(hardMaxAlive, baseMaxAlive + Mathf.FloorToInt(minutes * maxAliveGrowthPerMinute));
    }
}
