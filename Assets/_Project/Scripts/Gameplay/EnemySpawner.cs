using System;
using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    [Serializable]
    public class SpawnEntry
    {
        public Enemy prefab;
        public EnemyData data;
        [Min(0f)] public float weight = 1f;
        [Tooltip("Run minute from which this enemy can appear.")]
        public float unlockMinute;
        [Tooltip("How many spawn together in one clump (swarmers).")]
        [Min(1)] public int groupSize = 1;
    }

    /// <summary>
    /// Spawns regular enemies just outside the screen. What spawns (weighted table with unlock times),
    /// how often and how strong all come from <see cref="DifficultyConfig"/> and the elapsed run time.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private DifficultyConfig config;
        [SerializeField] private SpawnEntry[] entries;
        [SerializeField] private Camera viewCamera;
        [Tooltip("Extra distance beyond the screen corner where enemies appear.")]
        [SerializeField] private float margin = 2f;
        [SerializeField] private int prewarmPerEntry = 20;

        private ComponentPool<Enemy>[] _pools;
        private float _timer;

        private void Awake()
        {
            // The selected world decides who spawns; the inspector table is the fallback (and the test setup).
            var world = RunSettings.World;
            if (world != null && world.enemies != null && world.enemies.Length > 0)
                entries = world.enemies;

            _pools = new ComponentPool<Enemy>[entries.Length];
            for (var i = 0; i < entries.Length; i++)
                _pools[i] = new ComponentPool<Enemy>(entries[i].prefab, transform, prewarmPerEntry, 1000);
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || player.Health.IsDead)
                return;

            var minutes = RunClock.Instance != null ? RunClock.Instance.Minutes : 0f;
            var zone = ZoneTimer.Instance != null ? ZoneTimer.Instance.ZoneLevel : 1;
            var scale = config.EvaluateScale(minutes, zone);
            var world = RunSettings.World;
            if (world != null)
            {
                scale.Hp *= world.hpMultiplier;
                scale.Damage *= world.damageMultiplier;
            }
            var maxAlive = config.MaxAlive(minutes);

            _timer -= Time.deltaTime;
            var interval = config.SpawnInterval(minutes);
            while (_timer <= 0f)
            {
                _timer += interval;
                var batch = config.SpawnBatch(minutes);
                for (var i = 0; i < batch && Enemy.Active.Count < maxAlive; i++)
                    SpawnGroup(player.transform.position, minutes, scale);
            }
        }

        private int PickEntry(float minutes)
        {
            var total = 0f;
            for (var i = 0; i < entries.Length; i++)
                if (minutes >= entries[i].unlockMinute)
                    total += entries[i].weight;

            if (total <= 0f)
                return -1;

            var roll = UnityEngine.Random.value * total;
            for (var i = 0; i < entries.Length; i++)
            {
                if (minutes < entries[i].unlockMinute)
                    continue;
                roll -= entries[i].weight;
                if (roll <= 0f)
                    return i;
            }
            return 0;
        }

        private void SpawnGroup(Vector2 center, float minutes, EnemyScale scale)
        {
            var index = PickEntry(minutes);
            if (index < 0)
                return;

            var entry = entries[index];
            var halfH = viewCamera.orthographicSize;
            var halfW = halfH * viewCamera.aspect;
            var radius = Mathf.Sqrt(halfW * halfW + halfH * halfH) + margin;
            var angle = UnityEngine.Random.value * Mathf.PI * 2f;
            var spot = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            for (var g = 0; g < entry.groupSize; g++)
            {
                var enemy = _pools[index].Get();
                enemy.transform.position = entry.groupSize > 1 ? spot + UnityEngine.Random.insideUnitCircle * 1.2f : spot;
                enemy.Init(entry.data, _pools[index].Release, scale);
            }
        }
    }
}
