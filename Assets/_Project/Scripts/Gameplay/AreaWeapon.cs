using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public enum AreaMode
    {
        /// <summary>Instant blast(s) around the player. Level-table 'amount' = number of pulses per cast.</summary>
        Nova,
        /// <summary>Drops lingering damage zones (near enemies, at random spots, or under the player).</summary>
        Zones
    }

    public class AreaWeapon : Weapon
    {
        [SerializeField] private AreaMode mode;
        [SerializeField] private AreaZone zonePrefab;
        [Tooltip("Nova: ring sprite (radius 1 world unit at scale 1) that expands and fades on each blast.")]
        [SerializeField] private SpriteRenderer novaVisual;
        [SerializeField] private float novaVisualDuration = 0.3f;
        [Tooltip("Nova with several pulses: seconds between pulses.")]
        [SerializeField] private float pulseGap = 0.2f;
        [Tooltip("Zones: how far from the player zones may be placed / enemies are looked for.")]
        [SerializeField] private float placementRange = 6f;
        [Tooltip("Zones: place at random spots around the player instead of on the nearest enemy.")]
        [SerializeField] private bool randomPlacement;
        [Tooltip("Zones: drop the zones right under the player (mines / trail).")]
        [SerializeField] private bool atPlayer;

        private ComponentPool<AreaZone> _pool;
        private Color _novaColor = Color.white;
        private float _timer;
        private float _novaEndsAt;
        private int _pulsesLeft;
        private float _nextPulseAt;

        private void Awake()
        {
            if (zonePrefab != null)
                _pool = new ComponentPool<AreaZone>(zonePrefab, PoolRoot.Transform, 6, 300);
            if (novaVisual != null)
            {
                _novaColor = novaVisual.color;
                novaVisual.enabled = false;
            }
        }

        private void Update()
        {
            AnimateNova();

            if (!Ready)
                return;

            if (_pulsesLeft > 0 && Time.time >= _nextPulseAt)
            {
                _pulsesLeft--;
                _nextPulseAt = Time.time + pulseGap;
                Blast();
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            if (mode == AreaMode.Nova)
                TryNova();
            else
                TryZones();
        }

        private void TryNova()
        {
            Vector2 origin = transform.position;
            if (!EnemyQuery.TryGetNearestEnemy(origin, Area, out _))
                return;

            _timer = Cooldown;
            Blast();
            _pulsesLeft = Amount - 1;
            _nextPulseAt = Time.time + pulseGap;
        }

        private void Blast()
        {
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.WeaponArea);
            Vector2 origin = transform.position;
            var damage = Damage;
            var radius = Area;
            var radiusSqr = radius * radius;
            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (((Vector2)list[i].transform.position - origin).sqrMagnitude <= radiusSqr)
                    list[i].Health.TakeDamage(damage);
            }

            if (novaVisual != null)
            {
                novaVisual.enabled = true;
                _novaEndsAt = Time.time + novaVisualDuration;
            }
        }

        private void AnimateNova()
        {
            if (novaVisual == null || !novaVisual.enabled)
                return;

            var k = 1f - (_novaEndsAt - Time.time) / novaVisualDuration;
            if (k >= 1f)
            {
                novaVisual.enabled = false;
                return;
            }

            var scale = Area * Mathf.Lerp(0.3f, 1f, k);
            novaVisual.transform.localScale = new Vector3(scale, scale, 1f);
            var c = _novaColor;
            c.a *= 1f - k;
            novaVisual.color = c;
        }

        private void TryZones()
        {
            Vector2 origin = transform.position;
            if (!EnemyQuery.TryGetNearestEnemy(origin, placementRange, out var target))
                return;

            _timer = Cooldown;
            var damage = Damage;
            var count = Amount;
            var radius = Area;
            var duration = Duration;
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.WeaponArea);

            for (var i = 0; i < count; i++)
            {
                Vector2 pos;
                if (atPlayer)
                    pos = origin + Random.insideUnitCircle * (i == 0 ? 0f : radius * 0.8f);
                else if (randomPlacement)
                    pos = origin + Random.insideUnitCircle * placementRange;
                else if (i == 0)
                    pos = target.transform.position;
                else
                    pos = (Vector2)target.transform.position + Random.insideUnitCircle * radius;

                // speed = tick interval, duration = lifetime of the zone
                _pool.Get().Init(pos, radius, damage, Current.speed, duration, _pool.Release);
            }
        }
    }
}
