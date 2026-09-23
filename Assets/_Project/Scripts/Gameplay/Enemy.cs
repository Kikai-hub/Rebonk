using System;
using System.Collections.Generic;
using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        /// <summary>All live enemies. Iterate backwards: dying enemies remove themselves.</summary>
        public static readonly List<Enemy> Active = new List<Enemy>(512);

        /// <summary>Raised when an enemy is killed (before it returns to its pool).</summary>
        public static event Action<Enemy> Killed;

        /// <summary>Raised with the HP an enemy just lost (only the player damages enemies): drives lifesteal.</summary>
        public static event Action<float> Damaged;

        /// <summary>Live multipliers for spirit enemies, driven by <see cref="SpiritDirector"/> every frame of the survival phase.</summary>
        public static float SpiritSpeedMultiplier = 1f;
        public static float SpiritDamageMultiplier = 1f;

        private enum ChargeState { Walk, Windup, Dash }

        private static readonly Color WindupTint = new Color(1f, 0.4f, 0.4f, 1f);

        [SerializeField] private SpriteRenderer body;
        [Tooltip("Needs a material using the Rebonk/Sprite-Lit-Flash shader (flash is driven by the sprite color alpha).")]
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField, Range(0f, 1f)] private float flashStrength = 0.65f;
        [Header("Directional sprites (optional; empty = keep the single static sprite already on this prefab)")]
        [Tooltip("Facing the camera, moving toward the player.")]
        [SerializeField] private Sprite spriteDown;
        [Tooltip("Facing away from the camera, moving away from the player.")]
        [SerializeField] private Sprite spriteUp;
        [Tooltip("Facing right; mirrored for left via the flip below. No animation, just a turn.")]
        [SerializeField] private Sprite spriteSide;

        private EnemyData _data;
        private Action<Enemy> _release;
        private EnemyScale _scale = EnemyScale.One;
        private Color _tint = Color.white;
        private float _nextContactTime;
        private float _lastHp;
        private float _flashUntil;
        private bool _flashing;

        // behaviour state
        private float _shootTimer;
        private ChargeState _charge;
        private float _chargeCooldown;
        private float _stateTimer;
        private Vector2 _dashDirection;

        public Health Health { get; private set; }
        public EnemyData Data => _data;
        public EnemyScale Scale => _scale;
        public int XpValue => _data != null ? _data.xpValue : 0;
        public bool IsBoss => _data != null && _data.behavior == EnemyBehavior.Boss;

        /// <summary>Kills every regular enemy (bosses are skipped). Counts as real kills: XP drops, kill stats.</summary>
        public static void KillAllExceptBosses()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                if (i >= Active.Count)
                    continue;
                var e = Active[i];
                if (!e.IsBoss && !e.Health.IsDead)
                    e.Health.TakeDamage(e.Health.Current + 1f);
            }
        }

        private void Awake()
        {
            Health = GetComponent<Health>();
            Health.Died += OnDied;
            Health.Changed += OnHealthChanged;
        }

        private void OnEnable() => Active.Add(this);

        private void OnDisable()
        {
            Active.Remove(this);
            _flashing = false;
            SetTint(Color.white);
        }

        /// <param name="hpMultiplier">Applied to max HP at spawn (used by spirits, whose HP is fixed at spawn time).</param>
        public void Init(EnemyData data, Action<Enemy> release, float hpMultiplier = 1f)
        {
            Init(data, release, new EnemyScale { Hp = hpMultiplier, Speed = 1f, Damage = 1f });
        }

        public void Init(EnemyData data, Action<Enemy> release, EnemyScale scale)
        {
            _data = data;
            _release = release;
            _scale = scale;
            _nextContactTime = 0f;
            _shootTimer = UnityEngine.Random.Range(0.3f, Mathf.Max(0.4f, data.shootInterval));
            _charge = ChargeState.Walk;
            _chargeCooldown = UnityEngine.Random.Range(0.5f, Mathf.Max(0.6f, data.chargeCooldown));
            _flashing = false;
            SetTint(Color.white);

            var hp = data.maxHp * scale.Hp;
            _lastHp = hp;
            Health.Init(hp);
        }

        /// <summary>Returns this enemy to its pool without counting as a kill (no XP drop).</summary>
        public void Despawn()
        {
            var release = _release;
            _release = null;
            release?.Invoke(this);
        }

        public static void DespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
                Active[i].Despawn();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Active.Clear();
            SpiritSpeedMultiplier = 1f;
            SpiritDamageMultiplier = 1f;
        }

        private void SetTint(Color rgb)
        {
            _tint = rgb;
            if (body != null)
                body.color = new Color(rgb.r, rgb.g, rgb.b, 1f);
        }

        private void OnHealthChanged(float current, float max)
        {
            if (current < _lastHp)
                Damaged?.Invoke(_lastHp - current);

            if (current < _lastHp && body != null)
            {
                // Shader reads alpha as "flash amount": 1 = normal, lower = whiter.
                body.color = new Color(_tint.r, _tint.g, _tint.b, 1f - flashStrength);
                _flashUntil = Time.time + flashDuration;
                _flashing = true;
            }

            _lastHp = current;
        }

        private void Update()
        {
            if (_flashing && Time.time >= _flashUntil)
            {
                _flashing = false;
                SetTint(_tint);
            }

            var player = PlayerController.Instance;
            if (_data == null || player == null)
                return;

            Vector2 pos = transform.position;
            var toPlayer = (Vector2)player.transform.position - pos;
            var sqrDist = toPlayer.sqrMagnitude;
            var dist = Mathf.Sqrt(sqrDist);
            var dir = dist > 0.0001f ? toPlayer / dist : Vector2.zero;
            var speed = _data.moveSpeed * _scale.Speed * (_data.isSpirit ? SpiritSpeedMultiplier : 1f);

            switch (_data.behavior)
            {
                case EnemyBehavior.Ranged: UpdateRanged(pos, dir, dist, speed); break;
                case EnemyBehavior.Charge: UpdateCharge(dir, dist, speed); break;
                case EnemyBehavior.Boss: break; // BossController drives movement and attacks
                default: Move(dir * speed); break;
            }

            UpdateFacing(dir);

            if (sqrDist <= _data.contactRadius * _data.contactRadius && Time.time >= _nextContactTime)
            {
                var damage = _data.contactDamage * _scale.Damage * (_data.isSpirit ? SpiritDamageMultiplier : 1f);
                player.Health.TakeDamage(damage);
                _nextContactTime = Time.time + _data.contactCooldown;
            }
        }

        private void Move(Vector2 velocity) => transform.position += (Vector3)(velocity * Time.deltaTime);

        /// <summary>Turns to face <paramref name="dir"/>: picks the down/up/side sprite (no animation), mirrored for left.
        /// No-op (besides the flip) until the directional sprites are drawn; frozen during the charge windup telegraph.</summary>
        private void UpdateFacing(Vector2 dir)
        {
            if (body == null || _charge == ChargeState.Windup)
                return;

            if (spriteDown != null || spriteUp != null || spriteSide != null)
            {
                var vertical = Mathf.Abs(dir.y) > Mathf.Abs(dir.x);
                var sprite = vertical ? (dir.y > 0f ? spriteUp : spriteDown) : spriteSide;
                if (sprite != null)
                    body.sprite = sprite;
                body.flipX = !vertical && dir.x < 0f;
            }
            else if (Mathf.Abs(dir.x) > 0.01f)
            {
                body.flipX = dir.x < 0f;
            }
        }

        private void UpdateRanged(Vector2 pos, Vector2 dir, float dist, float speed)
        {
            var preferred = _data.preferredDistance;
            if (dist > preferred + 0.5f)
                Move(dir * speed);
            else if (dist < preferred - 1.5f)
                Move(-dir * (speed * 0.7f)); // back away when the player gets too close

            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0f && dist <= preferred + 4f)
            {
                _shootTimer = _data.shootInterval;
                EnemyProjectileManager.Fire(pos, dir, _data.projectileSpeed, _data.projectileDamage * _scale.Damage, _data.projectileLifetime);
            }
        }

        private void UpdateCharge(Vector2 dir, float dist, float speed)
        {
            switch (_charge)
            {
                case ChargeState.Walk:
                    Move(dir * speed);
                    _chargeCooldown -= Time.deltaTime;
                    if (_chargeCooldown <= 0f && dist <= _data.chargeTriggerRange)
                    {
                        _charge = ChargeState.Windup;
                        _stateTimer = _data.chargeWindup;
                        SetTint(WindupTint); // telegraph: red and standing still
                    }
                    break;

                case ChargeState.Windup:
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        _dashDirection = dir;
                        _charge = ChargeState.Dash;
                        _stateTimer = _data.chargeDuration;
                    }
                    break;

                default:
                    Move(_dashDirection * (_data.chargeSpeed * _scale.Speed));
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0f)
                    {
                        _charge = ChargeState.Walk;
                        _chargeCooldown = _data.chargeCooldown;
                        SetTint(Color.white);
                    }
                    break;
            }
        }

        private void OnDied(Health _)
        {
            Killed?.Invoke(this);
            _release?.Invoke(this);
        }
    }
}
