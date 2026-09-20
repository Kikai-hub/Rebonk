using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public enum ProjectileAim
    {
        /// <summary>Fan / shotgun spread around the direction to the nearest enemy.</summary>
        Nearest,
        /// <summary>Every projectile flies in a random direction (needs an enemy in range to trigger).</summary>
        RandomDirection,
        /// <summary>Volleys are spread evenly around the player and the pattern rotates each volley.</summary>
        Spiral,
        /// <summary>Evenly spaced full circle around the player, random start angle.</summary>
        Ring
    }

    /// <summary>Fires projectiles: at the nearest enemy (fan or random spread) or around the player (ring / spiral / random).</summary>
    public class ProjectileWeapon : Weapon
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private ProjectileAim aim = ProjectileAim.Nearest;
        [SerializeField] private float targetRange = 12f;
        [SerializeField] private float spreadDegrees = 12f;
        [Tooltip("Shotgun mode: every projectile gets a random angle inside the spread instead of an even fan.")]
        [SerializeField] private bool randomSpread;
        [SerializeField] private float spiralStepDegrees = 17f;
        [SerializeField] private float projectileLifetime = 1.6f;

        private ComponentPool<Projectile> _pool;
        private float _timer;
        private float _spiral;

        private void Awake()
        {
            _pool = new ComponentPool<Projectile>(projectilePrefab, PoolRoot.Transform, 10, 500);
        }

        private void Update()
        {
            if (!Ready)
                return;

            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            Vector2 origin = transform.position;
            if (!EnemyQuery.TryGetNearest(origin, targetRange, out var dir))
                return;

            _timer = Cooldown;
            Fire(origin, dir);
        }

        private void Fire(Vector2 origin, Vector2 dir)
        {
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.WeaponShot);
            var count = Amount;
            var speed = Speed;
            var pierce = Pierce;
            var bounces = Bounces;
            var baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var damage = Damage;

            if (aim == ProjectileAim.Spiral)
                _spiral += spiralStepDegrees;
            var ringOffset = Random.value * 360f;

            for (var i = 0; i < count; i++)
            {
                float degrees;
                switch (aim)
                {
                    case ProjectileAim.RandomDirection:
                        degrees = Random.value * 360f;
                        break;
                    case ProjectileAim.Spiral:
                        degrees = _spiral + i * 360f / count;
                        break;
                    case ProjectileAim.Ring:
                        degrees = ringOffset + i * 360f / count;
                        break;
                    default:
                        degrees = baseAngle + (randomSpread
                            ? Random.Range(-spreadDegrees * 0.5f, spreadDegrees * 0.5f)
                            : (i - (count - 1) * 0.5f) * spreadDegrees);
                        break;
                }

                var rad = degrees * Mathf.Deg2Rad;
                var d = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                _pool.Get().Init(origin, d, speed, damage, projectileLifetime, pierce, bounces, _pool.Release);
            }
        }
    }
}
