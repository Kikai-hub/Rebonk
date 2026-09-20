using System;
using System.Collections;
using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Reference boss. Phase 1: chase + ring of bullets, aimed volleys, telegraphed stomp.
    /// Phase 2 (below 50% HP): faster, bigger rings and it summons minions.
    /// Lives on the same object as <see cref="Enemy"/> so weapons hit it like any enemy.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class BossController : MonoBehaviour
    {
        private enum Pattern { Ring, Aimed, Stomp, Summon }

        private static readonly Pattern[] Phase1 = { Pattern.Ring, Pattern.Aimed, Pattern.Stomp };
        private static readonly Pattern[] Phase2 = { Pattern.Ring, Pattern.Stomp, Pattern.Summon, Pattern.Aimed };

        public static BossController Current { get; private set; }
        public static event Action<BossController> Spawned;
        public static event Action<BossController> Defeated;

        [SerializeField] private SpriteRenderer telegraph;
        [SerializeField] private Enemy minionPrefab;
        [SerializeField] private EnemyData minionData;

        [Header("Movement")]
        [SerializeField] private float chaseSeconds = 3f;
        [SerializeField] private float phase2ChaseSeconds = 2f;
        [SerializeField] private float phase2SpeedMultiplier = 1.5f;
        [SerializeField] private float keepDistance = 2f;

        [Header("Ring of bullets")]
        [SerializeField] private int ringCount = 14;
        [SerializeField] private int ringCountPhase2 = 20;
        [SerializeField] private float ringSpeed = 5f;
        [SerializeField] private float ringDamage = 8f;

        [Header("Aimed volleys")]
        [SerializeField] private int volleys = 3;
        [SerializeField] private int shotsPerVolley = 3;
        [SerializeField] private float volleySpreadDegrees = 10f;
        [SerializeField] private float aimedSpeed = 7f;
        [SerializeField] private float aimedDamage = 10f;

        [Header("Stomp")]
        [SerializeField] private float stompWindup = 1f;
        [SerializeField] private float stompRadius = 4.2f;
        [SerializeField] private float stompDamage = 25f;

        [Header("Summon")]
        [SerializeField] private int minionCount = 6;

        private const float BulletLifetime = 4f;

        private Enemy _enemy;
        private Health _health;
        private ComponentPool<Enemy> _minions;
        private bool _phase2;
        private bool _defeated;

        public string DisplayName => Loc.T(_enemy.Data != null ? _enemy.Data.displayName : "Boss");
        public EnemyData Data => _enemy != null ? _enemy.Data : null;
        public Health Health => _health;

        private void Start()
        {
            _enemy = GetComponent<Enemy>();
            _health = _enemy.Health;
            _health.Died += OnDied;
            if (telegraph != null)
                telegraph.enabled = false;

            Current = this;
            Spawned?.Invoke(this);
            StartCoroutine(Run());
        }

        private void OnDestroy()
        {
            if (Current == this)
                Current = null;
        }

        private void OnDied(Health _)
        {
            if (_defeated)
                return;
            _defeated = true;
            StopAllCoroutines();
            Defeated?.Invoke(this);
        }

        private void Update()
        {
            if (!_phase2 && _health != null && _health.Current <= _health.Max * 0.5f)
                _phase2 = true;
        }

        private IEnumerator Run()
        {
            yield return new WaitForSeconds(1f); // entrance beat
            var index = 0;
            while (true)
            {
                var chase = _phase2 ? phase2ChaseSeconds : chaseSeconds;
                for (var t = 0f; t < chase; t += Time.deltaTime)
                {
                    ChaseStep();
                    yield return null;
                }

                var list = _phase2 ? Phase2 : Phase1;
                switch (list[index++ % list.Length])
                {
                    case Pattern.Ring: yield return RingRoutine(); break;
                    case Pattern.Aimed: yield return AimedRoutine(); break;
                    case Pattern.Stomp: yield return StompRoutine(); break;
                    default: yield return SummonRoutine(); break;
                }
            }
        }

        private bool TryGetPlayer(out PlayerController player)
        {
            player = PlayerController.Instance;
            return player != null && !player.Health.IsDead;
        }

        private void ChaseStep()
        {
            if (!TryGetPlayer(out var player))
                return;

            Vector2 pos = transform.position;
            var to = (Vector2)player.transform.position - pos;
            var dist = to.magnitude;
            if (dist <= keepDistance)
                return;

            var speed = _enemy.Data.moveSpeed * _enemy.Scale.Speed * (_phase2 ? phase2SpeedMultiplier : 1f);
            transform.position += (Vector3)(to / dist * (speed * Time.deltaTime));
        }

        private float BulletDamage(float baseDamage) => baseDamage * _enemy.Scale.Damage;

        private IEnumerator RingRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            var count = _phase2 ? ringCountPhase2 : ringCount;
            var offset = UnityEngine.Random.value * Mathf.PI * 2f;
            for (var i = 0; i < count; i++)
            {
                var angle = offset + i * Mathf.PI * 2f / count;
                var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                EnemyProjectileManager.Fire(transform.position, dir, ringSpeed, BulletDamage(ringDamage), BulletLifetime);
            }
            yield return new WaitForSeconds(0.6f);
        }

        private IEnumerator AimedRoutine()
        {
            for (var v = 0; v < volleys; v++)
            {
                if (!TryGetPlayer(out var player))
                    yield break;

                var aim = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
                var baseAngle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
                for (var s = 0; s < shotsPerVolley; s++)
                {
                    var angle = (baseAngle + (s - (shotsPerVolley - 1) * 0.5f) * volleySpreadDegrees) * Mathf.Deg2Rad;
                    var dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    EnemyProjectileManager.Fire(transform.position, dir, aimedSpeed, BulletDamage(aimedDamage), BulletLifetime);
                }
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(0.4f);
        }

        private IEnumerator StompRoutine()
        {
            if (telegraph != null)
            {
                telegraph.enabled = true;
                telegraph.transform.localScale = new Vector3(stompRadius / Mathf.Max(0.01f, transform.localScale.x), stompRadius / Mathf.Max(0.01f, transform.localScale.y), 1f);
            }

            // Telegraph: pulsing red circle where the slam will land. The boss stands still.
            for (var t = 0f; t < stompWindup; t += Time.deltaTime)
            {
                if (telegraph != null)
                    telegraph.color = new Color(1f, 0.15f, 0.15f, 0.25f + 0.2f * Mathf.Sin(t * 20f) * 0.5f + 0.2f * (t / stompWindup));
                yield return null;
            }

            if (TryGetPlayer(out var player) &&
                ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude <= stompRadius * stompRadius)
            {
                player.Health.TakeDamage(BulletDamage(stompDamage));
            }

            if (telegraph != null)
            {
                telegraph.color = new Color(1f, 1f, 1f, 0.7f);
                yield return new WaitForSeconds(0.15f);
                telegraph.enabled = false;
            }
            yield return new WaitForSeconds(0.5f);
        }

        private IEnumerator SummonRoutine()
        {
            yield return new WaitForSeconds(0.4f);
            if (minionPrefab != null && minionData != null)
            {
                if (_minions == null)
                    _minions = new ComponentPool<Enemy>(minionPrefab, PoolRoot.Transform, 6, 60);

                for (var i = 0; i < minionCount; i++)
                {
                    var angle = i * Mathf.PI * 2f / minionCount;
                    var pos = (Vector2)transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 2.5f;
                    var minion = _minions.Get();
                    minion.transform.position = pos;
                    minion.Init(minionData, _minions.Release, _enemy.Scale);
                }
            }
            yield return new WaitForSeconds(0.8f);
        }
    }
}
