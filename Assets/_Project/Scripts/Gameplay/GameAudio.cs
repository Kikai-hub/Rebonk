using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Listens to what happens in the run and plays the matching sounds and music. Weapon shots, gem pickups
    /// and the portal call <see cref="Sound"/> themselves; everything driven by an event lives here.
    /// </summary>
    public class GameAudio : MonoBehaviour
    {
        private PlayerController _player;
        private float _lastHp;
        private bool _bossFight;

        private void OnEnable()
        {
            Enemy.Killed += OnEnemyKilled;
            Enemy.Damaged += OnEnemyDamaged;
            Inventory.WeaponEvolved += OnEvolved;
            BossController.Spawned += OnBossSpawned;
            BossController.Defeated += OnBossDefeated;
            Totem.Activated += OnTotem;
        }

        private void OnDisable()
        {
            Enemy.Killed -= OnEnemyKilled;
            Enemy.Damaged -= OnEnemyDamaged;
            Inventory.WeaponEvolved -= OnEvolved;
            BossController.Spawned -= OnBossSpawned;
            BossController.Defeated -= OnBossDefeated;
            Totem.Activated -= OnTotem;
        }

        private void Start()
        {
            _player = PlayerController.Instance;
            if (_player != null)
            {
                _lastHp = _player.Health.Current;
                _player.Health.Changed += OnPlayerHealth;
                _player.Health.Died += OnPlayerDied;
                _player.Level.LeveledUp += OnLevelUp;
            }

            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged += OnPhaseChanged;

            PlayMusicForPhase();
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.Health.Changed -= OnPlayerHealth;
                _player.Health.Died -= OnPlayerDied;
                _player.Level.LeveledUp -= OnLevelUp;
            }
            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged -= OnPhaseChanged;
        }

        private void OnEnemyKilled(Enemy _) => Sound.Play(SfxId.EnemyDie);
        private void OnEnemyDamaged(float _) => Sound.Play(SfxId.EnemyHit);
        private void OnEvolved() => Sound.Play(SfxId.Evolution);
        private void OnLevelUp(int _) => Sound.Play(SfxId.LevelUp);
        private void OnTotem(Totem _) => Sound.Play(SfxId.TotemDone);

        private void OnPlayerHealth(float current, float max)
        {
            if (current < _lastHp)
                Sound.Play(SfxId.PlayerHurt);
            _lastHp = current;
        }

        private void OnPlayerDied(Health _)
        {
            Sound.Play(SfxId.GameOver);
            Sound.PlayMusic(null, 2f);
        }

        private void OnBossSpawned(BossController _)
        {
            _bossFight = true;
            Sound.Play(SfxId.BossSpawn);
            Sound.PlayMusic(BossTrack());
        }

        private void OnBossDefeated(BossController _)
        {
            _bossFight = false;
            Sound.Play(SfxId.BossDefeated);
            PlayMusicForPhase();
        }

        private void OnPhaseChanged(ZonePhase phase)
        {
            if (phase == ZonePhase.Survival)
                Sound.Play(SfxId.SpiritsStart);
            else if (phase == ZonePhase.Cleared)
                Sound.Play(SfxId.ZoneCleared);

            if (!_bossFight)
                PlayMusicForPhase();
        }

        private void PlayMusicForPhase()
        {
            var world = RunSettings.World;
            var bank = Sound.Bank;
            var phase = ZoneTimer.Instance != null ? ZoneTimer.Instance.Phase : ZonePhase.Zone;

            AudioClip clip;
            if (phase == ZonePhase.Survival)
                clip = world != null && world.survivalMusic != null ? world.survivalMusic : bank != null ? bank.survivalMusic : null;
            else
                clip = world != null && world.zoneMusic != null ? world.zoneMusic : bank != null ? bank.defaultZoneMusic : null;
            Sound.PlayMusic(clip);
        }

        private AudioClip BossTrack()
        {
            var world = RunSettings.World;
            var bank = Sound.Bank;
            return world != null && world.bossMusic != null ? world.bossMusic : bank != null ? bank.bossMusic : null;
        }
    }
}
