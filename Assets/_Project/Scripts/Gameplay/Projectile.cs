using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Pooled flying projectile. Passes through <c>pierce</c> enemies (each hit once), can steer toward enemies
    /// (homing) and can jump to another enemy after a hit (bounces), then despawns.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float hitRadius = 0.45f;
        [Tooltip("Degrees per second the projectile turns toward the nearest enemy (0 = flies straight).")]
        [SerializeField] private float homingTurnRate;
        [SerializeField] private float homingRange = 10f;
        [SerializeField] private float bounceRange = 9f;

        private readonly List<Enemy> _hit = new List<Enemy>(8);
        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _lifeLeft;
        private int _pierceLeft;
        private int _bouncesLeft;
        private Action<Projectile> _release;

        public void Init(Vector2 position, Vector2 direction, float speed, float damage, float lifetime, int pierce, int bounces, Action<Projectile> release)
        {
            transform.position = position;
            _direction = direction;
            Face();
            _speed = speed;
            _damage = damage;
            _lifeLeft = lifetime;
            _pierceLeft = Mathf.Max(1, pierce);
            _bouncesLeft = Mathf.Max(0, bounces);
            _hit.Clear();
            _release = release;
        }

        private void Face() =>
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);

        private void Update()
        {
            if (_release == null)
                return;

            if (homingTurnRate > 0f)
                Steer();

            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));

            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f)
            {
                Despawn();
                return;
            }

            Vector2 pos = transform.position;
            var radiusSqr = hitRadius * hitRadius;
            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var enemy = list[i];
                if (((Vector2)enemy.transform.position - pos).sqrMagnitude > radiusSqr || _hit.Contains(enemy))
                    continue;

                _hit.Add(enemy);
                enemy.Health.TakeDamage(_damage);

                // A bounce keeps the projectile alive and points it at a fresh target.
                if (_bouncesLeft > 0 && TryFindTarget(pos, bounceRange, out var next))
                {
                    _bouncesLeft--;
                    _direction = ((Vector2)next.transform.position - pos).normalized;
                    Face();
                    continue;
                }

                if (--_pierceLeft <= 0)
                {
                    Despawn();
                    return;
                }
            }
        }

        private void Steer()
        {
            Vector2 pos = transform.position;
            if (!TryFindTarget(pos, homingRange, out var target))
                return;

            var wanted = ((Vector2)target.transform.position - pos).normalized;
            var angle = Vector2.SignedAngle(_direction, wanted);
            var step = Mathf.Clamp(angle, -homingTurnRate * Time.deltaTime, homingTurnRate * Time.deltaTime);
            _direction = Quaternion.Euler(0f, 0f, step) * _direction;
            Face();
        }

        /// <summary>Nearest live enemy within range that this projectile has not hit yet.</summary>
        private bool TryFindTarget(Vector2 from, float range, out Enemy target)
        {
            var best = range * range;
            target = null;
            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var e = list[i];
                if (_hit.Contains(e))
                    continue;
                var sq = ((Vector2)e.transform.position - from).sqrMagnitude;
                if (sq >= best)
                    continue;
                best = sq;
                target = e;
            }
            return target != null;
        }

        private void Despawn()
        {
            var release = _release;
            _release = null;
            release(this);
        }
    }
}
