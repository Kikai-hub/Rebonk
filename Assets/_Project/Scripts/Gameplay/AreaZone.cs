using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Pooled lingering damage zone: damages every enemy inside it once per tick until it expires.</summary>
    public class AreaZone : MonoBehaviour
    {
        [Tooltip("Filled-circle sprite whose radius is 1 world unit at scale 1 (32px at PPU 16).")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private float fadeSeconds = 0.5f;

        private Action<AreaZone> _release;
        private Color _baseColor = Color.white;
        private float _radius;
        private float _damage;
        private float _tickInterval;
        private float _tickTimer;
        private float _lifeLeft;

        public void Init(Vector2 position, float radius, float damage, float tickInterval, float duration, Action<AreaZone> release)
        {
            transform.position = position;
            transform.localScale = new Vector3(radius, radius, 1f);
            _radius = radius;
            _damage = damage;
            _tickInterval = Mathf.Max(0.05f, tickInterval);
            _tickTimer = _tickInterval * 0.5f;
            _lifeLeft = duration;
            _release = release;

            if (body != null)
            {
                if (_baseColor == Color.white)
                    _baseColor = body.color;
                body.color = _baseColor;
            }
        }

        private void Update()
        {
            if (_release == null)
                return;

            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f)
            {
                var release = _release;
                _release = null;
                release(this);
                return;
            }

            if (body != null && _lifeLeft < fadeSeconds)
            {
                var c = _baseColor;
                c.a *= _lifeLeft / fadeSeconds;
                body.color = c;
            }

            _tickTimer -= Time.deltaTime;
            if (_tickTimer > 0f)
                return;

            _tickTimer += _tickInterval;
            Vector2 pos = transform.position;
            var radiusSqr = _radius * _radius;
            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (((Vector2)list[i].transform.position - pos).sqrMagnitude <= radiusSqr)
                    list[i].Health.TakeDamage(_damage);
            }
        }
    }
}
