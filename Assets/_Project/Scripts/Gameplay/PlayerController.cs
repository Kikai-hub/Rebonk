using System;
using Rebonk.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rebonk.Gameplay
{
    [RequireComponent(typeof(Health), typeof(PlayerStats), typeof(PlayerLevel))]
    public class PlayerController : MonoBehaviour
    {
        [FormerlySerializedAs("stats")]
        [SerializeField] private CharacterStats baseStats;
        [SerializeField] private SpriteRenderer body;
        [Tooltip("Short invulnerability after taking a hit, so a crowd cannot stack damage in one instant. The sprite blinks meanwhile.")]
        [SerializeField] private float hitGraceSeconds = 0.35f;

        /// <summary>Collision radius against map obstacles (world units).</summary>
        public const float BodyRadius = 0.4f;
        private const float ReviveGraceSeconds = 2f;

        public static PlayerController Instance { get; private set; }

        /// <summary>Character definition (design-time values).</summary>
        public CharacterStats Base => baseStats;
        /// <summary>Runtime stats with passives and blessings applied.</summary>
        public PlayerStats Stats { get; private set; }
        public PlayerLevel Level { get; private set; }
        public Health Health { get; private set; }
        public Vector2 Facing { get; private set; } = Vector2.right;
        /// <summary>Extra lives left: the character's own plus those from items, minus the ones already used.</summary>
        public int RevivesLeft => Mathf.Max(0, baseStats.revives + Stats.ExtraLives - _revivesUsed);

        /// <summary>Raised when an extra life saves the player.</summary>
        public event Action Revived;

        public bool IsSpeedBoosted => Time.time < _boostUntil;
        public float SpeedBoostSecondsLeft => Mathf.Max(0f, _boostUntil - Time.time);

        /// <summary>Temporary move-speed multiplier. Picking up another boost refreshes the timer instead of stacking.</summary>
        public void ApplySpeedBoost(float multiplier, float seconds)
        {
            _boostMultiplier = Mathf.Max(multiplier, IsSpeedBoosted ? _boostMultiplier : 1f);
            _boostUntil = Mathf.Max(_boostUntil, Time.time + seconds);
        }

        private static readonly Color BoostTint = new Color(0.65f, 1f, 1f, 1f);

        private float _graceUntil;
        private float _boostMultiplier = 1f;
        private float _boostUntil;
        private bool _boostTinted;
        private float _lastHp;
        private bool _blinking;
        private int _revivesUsed;

        // walk animation: 0 = down (toward camera), 1 = up (away), 2 = side (mirrored for left)
        private int _animDir = 2;
        private float _animT;

        private void Awake()
        {
            Instance = this;

            // The character chosen in the menu overrides the scene default.
            if (RunSettings.Character != null)
                baseStats = RunSettings.Character;
            if (body != null && baseStats.sprite != null)
                body.sprite = baseStats.sprite;

            Health = GetComponent<Health>();
            Stats = GetComponent<PlayerStats>();
            Level = GetComponent<PlayerLevel>();

            Stats.Init(baseStats);
            Health.Init(Stats.MaxHp);
            ApplyDefensiveStats();
            Health.BeforeDeath = TryRevive;
            _lastHp = Health.Current;
            Health.Changed += OnHealthChanged;

            Stats.Changed += () =>
            {
                Health.SetMax(Stats.MaxHp);
                ApplyDefensiveStats();
            };
        }

        private void OnEnable() => Enemy.Damaged += OnEnemyDamaged;

        private void OnDisable() => Enemy.Damaged -= OnEnemyDamaged;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnHealthChanged(float current, float max)
        {
            if (current < _lastHp && current > 0f && hitGraceSeconds > 0f)
            {
                Health.Invulnerable = true;
                _graceUntil = Mathf.Max(_graceUntil, Time.time + hitGraceSeconds);
            }
            _lastHp = current;
        }

        private void ApplyDefensiveStats()
        {
            Health.FlatReduction = Stats.Armor;
            Health.DodgeChance = Stats.DodgeChance;
        }

        /// <summary>Vampirism: heals a fraction of all damage dealt to enemies.</summary>
        private void OnEnemyDamaged(float amount)
        {
            if (Stats != null && Stats.Lifesteal > 0f && !Health.IsDead)
                Health.Heal(amount * Stats.Lifesteal);
        }

        private void UpdateBlink()
        {
            if (body == null)
                return;
            if (Health.Invulnerable)
            {
                _blinking = true;
                var c = body.color;
                c.a = Mathf.Repeat(Time.time * 12f, 1f) < 0.5f ? 0.35f : 1f;
                body.color = c;
            }
            else if (_blinking)
            {
                _blinking = false;
                var c = body.color;
                c.a = 1f;
                body.color = c;
            }
        }

        private void UpdateBoostTint()
        {
            var boosted = IsSpeedBoosted;
            if (body == null || (!boosted && !_boostTinted))
                return;

            _boostTinted = boosted;
            // Pulses instead of a steady tint: a constant cyan multiply makes warm-colored heroes look sickly green.
            var rgb = boosted ? Color.Lerp(Color.white, BoostTint, 0.5f + 0.5f * Mathf.Sin(Time.time * 14f)) : Color.white;
            body.color = new Color(rgb.r, rgb.g, rgb.b, body.color.a);
        }

        private bool TryRevive(Health health)
        {
            if (RevivesLeft <= 0)
                return false;

            _revivesUsed++;
            health.Restore(0.5f);
            health.Invulnerable = true;
            _graceUntil = Time.time + ReviveGraceSeconds;
            Revived?.Invoke();
            return true;
        }

        private void Update()
        {
            if (Health.IsDead)
                return;

            if (Health.Invulnerable && Time.time >= _graceUntil)
                Health.Invulnerable = false;

            UpdateBlink();
            UpdateBoostTint();
            MinimapModel.Reveal(transform.position);

            if (Stats.RegenPerSecond > 0f)
                Health.Heal(Stats.RegenPerSecond * Time.deltaTime);

            var move = MoveInput.Value;
            if (move.sqrMagnitude < 0.0001f)
                return;

            var speed = Stats.MoveSpeed * (IsSpeedBoosted ? _boostMultiplier : 1f);
            var next = (Vector2)transform.position + move * (speed * Time.deltaTime);
            next = ObstacleMap.Resolve(next, BodyRadius); // slide along solid obstacles
            transform.position = new Vector3(next.x, next.y, transform.position.z);
            Facing = move.normalized;

            UpdateWalkAnimation(move);
        }

        /// <summary>Picks down/up/side frames by the dominant movement axis and cycles them; freezes on the last frame when idle.</summary>
        private void UpdateWalkAnimation(Vector2 move)
        {
            if (body == null)
                return;

            var newDir = Mathf.Abs(move.y) > Mathf.Abs(move.x) ? (move.y > 0f ? 1 : 0) : 2;
            if (newDir != _animDir)
            {
                _animDir = newDir;
                _animT = 0f;
            }

            var frames = _animDir == 0 ? baseStats.framesDown : _animDir == 1 ? baseStats.framesUp : baseStats.framesSide;
            if (frames == null || frames.Length == 0)
            {
                body.sprite = baseStats.sprite;
            }
            else
            {
                _animT += Time.deltaTime * Mathf.Max(0.01f, baseStats.animFps);
                body.sprite = frames[Mathf.FloorToInt(_animT) % frames.Length];
            }

            // Down/up frames are drawn facing the camera (no mirroring); side frames are drawn facing right.
            body.flipX = _animDir == 2 && move.x < 0f;
        }
    }
}
