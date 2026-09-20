using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Flies out to a max distance, then returns to the owner. Hits each enemy once per leg (out / back).</summary>
    public class BoomerangProjectile : MonoBehaviour
    {
        [SerializeField] private float hitRadius = 0.6f;
        [SerializeField] private float spinDegreesPerSecond = 720f;
        [SerializeField] private float returnSpeedMultiplier = 1.2f;

        private const float CatchDistance = 0.5f;

        private readonly List<Enemy> _hit = new List<Enemy>(16);
        private Transform _owner;
        private Vector2 _origin;
        private Vector2 _direction;
        private float _range;
        private float _speed;
        private float _damage;
        private bool _returning;
        private Action<BoomerangProjectile> _release;

        public void Init(Transform owner, Vector2 direction, float range, float speed, float damage, Action<BoomerangProjectile> release)
        {
            _owner = owner;
            _origin = owner.position;
            transform.position = _origin;
            _direction = direction;
            _range = range;
            _speed = speed;
            _damage = damage;
            _returning = false;
            _hit.Clear();
            _release = release;
        }

        private void Update()
        {
            if (_release == null)
                return;
            if (_owner == null)
            {
                Despawn();
                return;
            }

            transform.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);

            Vector2 pos = transform.position;
            if (!_returning)
            {
                pos += _direction * (_speed * Time.deltaTime);
                if ((pos - _origin).sqrMagnitude >= _range * _range)
                {
                    _returning = true;
                    _hit.Clear();
                }
            }
            else
            {
                var to = (Vector2)_owner.position - pos;
                var dist = to.magnitude;
                var step = _speed * returnSpeedMultiplier * Time.deltaTime;
                if (dist <= CatchDistance || step >= dist)
                {
                    Despawn();
                    return;
                }
                pos += to / dist * step;
            }
            transform.position = pos;

            var radiusSqr = hitRadius * hitRadius;
            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var enemy = list[i];
                if (((Vector2)enemy.transform.position - pos).sqrMagnitude > radiusSqr || _hit.Contains(enemy))
                    continue;

                _hit.Add(enemy);
                enemy.Health.TakeDamage(_damage);
            }
        }

        private void Despawn()
        {
            var release = _release;
            _release = null;
            release(this);
        }
    }
}
