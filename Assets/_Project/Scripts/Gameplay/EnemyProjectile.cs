using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Pooled projectile fired by ranged enemies; damages the player on touch.</summary>
    public class EnemyProjectile : MonoBehaviour
    {
        [SerializeField] private float hitRadius = 0.35f;

        private static readonly System.Collections.Generic.List<EnemyProjectile> Active = new System.Collections.Generic.List<EnemyProjectile>(128);

        private Vector2 _direction;
        private float _speed;
        private float _damage;
        private float _lifeLeft;
        private Action<EnemyProjectile> _release;

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        public static void DespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var p = Active[i];
                var release = p._release;
                p._release = null;
                release?.Invoke(p);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();

        public void Init(Vector2 position, Vector2 direction, float speed, float damage, float lifetime, Action<EnemyProjectile> release)
        {
            transform.position = position;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            _direction = direction;
            _speed = speed;
            _damage = damage;
            _lifeLeft = lifetime;
            _release = release;
        }

        private void Update()
        {
            if (_release == null)
                return;

            transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));

            _lifeLeft -= Time.deltaTime;
            var player = PlayerController.Instance;
            var hit = player != null && !player.Health.IsDead &&
                      ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude <= hitRadius * hitRadius;

            if (hit)
                player.Health.TakeDamage(_damage);

            if (hit || _lifeLeft <= 0f)
            {
                var release = _release;
                _release = null;
                release(this);
            }
        }
    }
}
