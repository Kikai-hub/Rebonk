using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public enum ZonePhase
    {
        /// <summary>Normal play: regular enemies, zone timer counting down.</summary>
        Zone,
        /// <summary>Zone timer ran out: regular spawns stop, spirits come from every side, forever getting stronger.
        /// Never ends on its own; the goal is to last as long as possible.</summary>
        Survival,
        /// <summary>Boss portal used; short pause before the next zone starts.</summary>
        Cleared
    }

    /// <summary>
    /// Drives one zone: timer (10-12 min) → endless spirit survival. The ONLY way to the next zone is
    /// <see cref="CompleteZone"/>, called by the portal that appears where the boss died (Stage 6).
    /// Player progress (level, weapons) is kept across zones; only the map state is reset.
    /// </summary>
    public class ZoneTimer : MonoBehaviour
    {
        [SerializeField] private float zoneDurationSeconds = 660f;
        [SerializeField] private float clearedPauseSeconds = 3f;
        [SerializeField] private EnemySpawner enemySpawner;
        [SerializeField] private SpiritDirector spiritDirector;

        public static ZoneTimer Instance { get; private set; }

        public int ZoneLevel { get; private set; } = 1;
        public ZonePhase Phase { get; private set; }
        /// <summary>Seconds left in the current timed phase (Zone / Cleared).</summary>
        public float TimeLeft { get; private set; }
        /// <summary>Seconds survived since the spirits appeared (Survival phase); counts up.</summary>
        public float SurvivedSeconds { get; private set; }

        public event Action<ZonePhase> PhaseChanged;

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start() => BeginZone();

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || player.Health.IsDead)
                return;

            if (Phase == ZonePhase.Survival)
            {
                SurvivedSeconds += Time.deltaTime;
                return;
            }

            TimeLeft -= Time.deltaTime;
            if (TimeLeft > 0f)
                return;

            if (Phase == ZonePhase.Zone)
            {
                BeginSurvival();
            }
            else
            {
                ZoneLevel++;
                BeginZone();
            }
        }

        /// <summary>Leaves the current zone (boss portal entered). Next zone starts after a short pause.</summary>
        [ContextMenu("Complete Zone (debug)")]
        public void CompleteZone()
        {
            if (Phase == ZonePhase.Cleared)
                return;

            Phase = ZonePhase.Cleared;
            TimeLeft = clearedPauseSeconds;
            spiritDirector.End();
            Enemy.DespawnAll();

            var health = PlayerController.Instance.Health;
            health.Heal(health.Max);
            PhaseChanged?.Invoke(Phase);
        }

        private void BeginZone()
        {
            Phase = ZonePhase.Zone;
            TimeLeft = zoneDurationSeconds;
            SurvivedSeconds = 0f;
            enemySpawner.enabled = true;
            PhaseChanged?.Invoke(Phase);
        }

        private void BeginSurvival()
        {
            Phase = ZonePhase.Survival;
            SurvivedSeconds = 0f;
            enemySpawner.enabled = false;
            spiritDirector.Begin();
            PhaseChanged?.Invoke(Phase);
        }
    }
}
