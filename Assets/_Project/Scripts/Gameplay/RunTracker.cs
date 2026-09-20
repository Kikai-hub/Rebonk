using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public struct RunResult
    {
        public int Kills;
        public int ZonesCleared;
        public float SurvivalSeconds;
        public float RunSeconds;
        public int Level;
        public int Evolutions;
        public int Bosses;
        public List<string> NewUnlocks;
        /// <summary>Already localized descriptions of daily quests finished by this run.</summary>
        public List<string> QuestsCompleted;
        /// <summary>English names of achievements earned by this run.</summary>
        public List<string> NewAchievements;
    }

    /// <summary>
    /// Counts what happens during a run and, when it ends (death, or leaving via MENU), folds it into the
    /// saved profile, unlocks whatever conditions are now met and saves.
    /// </summary>
    public class RunTracker : MonoBehaviour
    {
        [SerializeField] private MetaCatalog catalog;

        public static RunTracker Instance { get; private set; }

        public RunResult LastResult { get; private set; }
        public bool Finished => _finished;
        public int Kills => _kills;
        public int ZonesCleared => _zones;

        private readonly List<string> _bosses = new List<string>();
        private int _kills;
        private int _zones;
        private int _evolutions;
        private float _survival;
        private bool _finished;

        private void Awake() => Instance = this;

        private void OnEnable()
        {
            Enemy.Killed += OnEnemyKilled;
            BossController.Defeated += OnBossDefeated;
            Inventory.WeaponEvolved += OnWeaponEvolved;
        }

        private void OnDisable()
        {
            Enemy.Killed -= OnEnemyKilled;
            BossController.Defeated -= OnBossDefeated;
            Inventory.WeaponEvolved -= OnWeaponEvolved;
        }

        private void Start()
        {
            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged -= OnPhaseChanged;
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (!_finished && ZoneTimer.Instance != null)
                _survival = Mathf.Max(_survival, ZoneTimer.Instance.SurvivedSeconds);
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            if (!_finished)
                _kills++;
        }

        private void OnBossDefeated(BossController boss)
        {
            if (!_finished && boss.Data != null)
                _bosses.Add(boss.Data.name);
        }

        private void OnWeaponEvolved()
        {
            if (!_finished)
                _evolutions++;
        }

        private void OnPhaseChanged(ZonePhase phase)
        {
            if (!_finished && phase == ZonePhase.Cleared)
                _zones++;
        }

        /// <summary>Ends the run once: updates totals and bests, unlocks, saves. Safe to call repeatedly.</summary>
        public RunResult FinishRun()
        {
            if (_finished)
                return LastResult;
            _finished = true;

            var level = PlayerController.Instance != null ? PlayerController.Instance.Level.Level : 1;
            var runSeconds = RunClock.Instance != null ? RunClock.Instance.Elapsed : 0f;

            var data = SaveSystem.Data;
            data.totalRuns++;
            data.totalKills += _kills;
            data.totalZones += _zones;
            data.totalEvolutions += _evolutions;
            data.totalPlaySeconds += runSeconds;
            data.bestKills = Mathf.Max(data.bestKills, _kills);
            data.bestZones = Mathf.Max(data.bestZones, _zones);
            data.bestSurvival = Mathf.Max(data.bestSurvival, _survival);
            data.bestLevel = Mathf.Max(data.bestLevel, level);
            data.bestRunSeconds = Mathf.Max(data.bestRunSeconds, runSeconds);
            foreach (var b in _bosses)
                data.AddBossKill(b);

            var result = new RunResult
            {
                Kills = _kills,
                ZonesCleared = _zones,
                SurvivalSeconds = _survival,
                RunSeconds = runSeconds,
                Level = level,
                Evolutions = _evolutions,
                Bosses = _bosses.Count
            };

            // Quests first (they feed achievements and unlocks), achievements next (they feed unlocks).
            result.QuestsCompleted = DailyQuests.Apply(data, result);
            result.NewAchievements = Achievements.Evaluate(data);
            result.NewUnlocks = catalog != null ? catalog.EvaluateUnlocks(data) : new List<string>();
            SaveSystem.Save();

            LastResult = result;
            return LastResult;
        }
    }
}
