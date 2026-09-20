using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Throws boomerangs toward the nearest enemy; they fly out, turn around and come back, hitting everything on the way.</summary>
    public class BoomerangWeapon : Weapon
    {
        [SerializeField] private BoomerangProjectile projectilePrefab;
        [SerializeField] private float fanDegrees = 18f;
        [Tooltip("An enemy must be within (max distance x this) to trigger a throw.")]
        [SerializeField] private float triggerRangeFactor = 1.3f;

        private ComponentPool<BoomerangProjectile> _pool;
        private float _timer;

        private void Awake()
        {
            _pool = new ComponentPool<BoomerangProjectile>(projectilePrefab, PoolRoot.Transform, 6, 100);
        }

        private void Update()
        {
            if (!Ready)
                return;

            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            Vector2 origin = transform.position;
            var range = Area;
            if (!EnemyQuery.TryGetNearest(origin, range * triggerRangeFactor, out var dir))
                return;

            _timer = Cooldown;
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.WeaponBoomerang);
            var count = Amount;
            var speed = Speed;
            var baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var damage = Damage;

            for (var i = 0; i < count; i++)
            {
                var angle = (baseAngle + (i - (count - 1) * 0.5f) * fanDegrees) * Mathf.Deg2Rad;
                var d = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                _pool.Get().Init(Player.transform, d, range, speed, damage, _pool.Release);
            }
        }
    }
}
