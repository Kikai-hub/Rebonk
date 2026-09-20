using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>A constant field around the player that damages every enemy inside it once per tick.</summary>
    public class AuraWeapon : Weapon
    {
        [Tooltip("Filled-circle sprite (radius 1 world unit at scale 1) showing the field; tinted by its own color.")]
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private float pulseAmount = 0.05f;

        private float _tick;
        private Color _baseColor = Color.white;

        private void Awake()
        {
            if (visual != null)
                _baseColor = visual.color;
        }

        private void Update()
        {
            if (Data == null)
                return;

            var radius = Area;
            if (visual != null)
            {
                var s = radius * (1f + Mathf.Sin(Time.time * 3f) * pulseAmount);
                visual.transform.localScale = new Vector3(s, s, 1f);
                visual.enabled = !Player.Health.IsDead;
            }

            if (!Ready)
                return;

            _tick -= Time.deltaTime;
            if (_tick > 0f)
                return;

            _tick = Cooldown;
            Vector2 origin = transform.position;
            var damage = Damage;
            var radiusSqr = radius * radius;
            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (((Vector2)list[i].transform.position - origin).sqrMagnitude <= radiusSqr)
                    list[i].Health.TakeDamage(damage);
            }

            // brief brighter flash on every tick
            if (visual != null)
            {
                var c = _baseColor;
                c.a = Mathf.Min(1f, c.a * 1.8f);
                visual.color = c;
            }
        }

        private void LateUpdate()
        {
            if (visual != null)
                visual.color = Color.Lerp(visual.color, _baseColor, 10f * Time.deltaTime);
        }
    }
}
