using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Blades circle the player and damage enemies they touch, once per tick.</summary>
    public class OrbitWeapon : Weapon
    {
        [SerializeField] private Sprite bladeSprite;
        [SerializeField] private float bladeHitRadius = 0.6f;
        [SerializeField] private float bladeScale = 1f;
        [SerializeField] private Color bladeColor = Color.white;

        private readonly List<Transform> _blades = new List<Transform>();
        private float _angle;
        private float _tickTimer;

        /// <summary>Blades come from the level table and from items / blessings, so check every frame (cheap).</summary>
        private void EnsureBlades()
        {
            var wanted = Amount;
            while (_blades.Count < wanted)
            {
                var go = new GameObject("Blade");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = bladeSprite;
                sr.sortingOrder = 9;
                sr.color = bladeColor;
                go.transform.localScale = Vector3.one * bladeScale;
                _blades.Add(go.transform);
            }
        }

        private void Update()
        {
            if (!Ready)
                return;

            EnsureBlades();
            var count = _blades.Count;
            var radius = Area;
            _angle += Speed * Time.deltaTime;
            for (var i = 0; i < count; i++)
            {
                var a = (_angle + i * 360f / count) * Mathf.Deg2Rad;
                _blades[i].localPosition = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                _blades[i].localRotation = Quaternion.Euler(0f, 0f, _angle * 2f);
            }

            _tickTimer -= Time.deltaTime;
            if (_tickTimer > 0f)
                return;

            _tickTimer = Cooldown;
            var damage = Damage;
            var hit = bladeHitRadius * Player.Stats.AreaMultiplier;
            var radiusSqr = hit * hit;
            var list = Enemy.Active;
            for (var b = 0; b < count; b++)
            {
                Vector2 bladePos = _blades[b].position;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (((Vector2)list[i].transform.position - bladePos).sqrMagnitude <= radiusSqr)
                        list[i].Health.TakeDamage(damage);
                }
            }
        }
    }
}
