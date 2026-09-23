using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Spawns waves of spirits from every side during the endless survival phase.
    /// Strength grows with elapsed survival SECONDS (not with wave number) and has no time cap:
    /// multiplier(t) = 1 + ratePerSecond * t, for HP (fixed at spawn), speed and contact damage (live).
    /// </summary>
    public class SpiritDirector : MonoBehaviour
    {
        [SerializeField] private Enemy spiritPrefab;
        [SerializeField] private EnemyData spiritData;
        [SerializeField] private Camera viewCamera;

        [Header("Waves")]
        [SerializeField] private float startWaveInterval = 2.5f;
        [SerializeField] private float endWaveInterval = 0.8f;
        [SerializeField] private int startWaveSize = 6;
        [SerializeField] private float waveSizeGrowthPerSecond = 0.2f;
        [SerializeField] private int maxAlive = 300;
        [SerializeField] private float margin = 2f;
        [SerializeField] private int prewarm = 60;

        [Header("Strength growth per second of survival")]
        [SerializeField] private float hpPerSecond = 0.03f;
        [SerializeField] private float damagePerSecond = 0.03f;
        [SerializeField] private float speedPerSecond = 0.006f;

        [Tooltip("Seconds over which the wave interval shrinks from start to end value; strength keeps growing after that.")]
        [SerializeField] private float waveRampSeconds = 120f;

        private ComponentPool<Enemy> _pool;
        private bool _active;
        private float _elapsed;
        private float _waveTimer;

        public bool IsActive => _active;
        public float Elapsed => _elapsed;

        private void Awake()
        {
            _pool = new ComponentPool<Enemy>(spiritPrefab, transform, prewarm, maxAlive * 2);
        }

        private void OnDestroy() => End();

        public void Begin()
        {
            _active = true;
            _elapsed = 0f;
            _waveTimer = 0f;
            ApplyLiveMultipliers();
        }

        public void End()
        {
            _active = false;
            Enemy.SpiritSpeedMultiplier = 1f;
            Enemy.SpiritDamageMultiplier = 1f;
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (!_active || player == null || player.Health.IsDead)
                return;

            _elapsed += Time.deltaTime;
            ApplyLiveMultipliers();

            _waveTimer -= Time.deltaTime;
            if (_waveTimer > 0f)
                return;

            _waveTimer = Mathf.Lerp(startWaveInterval, endWaveInterval, Mathf.Clamp01(_elapsed / Mathf.Max(1f, waveRampSeconds)));
            SpawnWave(player.transform.position);
        }

        private void ApplyLiveMultipliers()
        {
            // Some characters (Ghost Gary) make the spirits hit softer and grow slower.
            var character = PlayerController.Instance != null ? PlayerController.Instance.Base : null;
            var growth = character != null ? character.spiritGrowthMultiplier : 1f;
            var softness = character != null ? character.spiritDamageMultiplier : 1f;

            Enemy.SpiritSpeedMultiplier = 1f + speedPerSecond * _elapsed * growth;
            Enemy.SpiritDamageMultiplier = (1f + damagePerSecond * _elapsed * growth) * softness;
        }

        private void SpawnWave(Vector2 center)
        {
            var count = Mathf.RoundToInt(startWaveSize + waveSizeGrowthPerSecond * _elapsed);
            var halfH = viewCamera.orthographicSize;
            var halfW = halfH * viewCamera.aspect;
            var radius = Mathf.Sqrt(halfW * halfW + halfH * halfH) + margin;
            var growth = PlayerController.Instance != null ? PlayerController.Instance.Base.spiritGrowthMultiplier : 1f;
            var hpMultiplier = 1f + hpPerSecond * _elapsed * growth;

            // Evenly spread around the player = "from every side".
            var step = Mathf.PI * 2f / count;
            var offset = Random.value * Mathf.PI * 2f;
            for (var i = 0; i < count && Enemy.Active.Count < maxAlive; i++)
            {
                var angle = offset + i * step + Random.Range(-0.15f, 0.15f);
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                var spirit = _pool.Get();
                spirit.transform.position = ObstacleMap.ArenaSpawnPoint(center, center + dir * radius);
                spirit.Init(spiritData, _pool.Release, hpMultiplier);
            }
        }
    }
}
