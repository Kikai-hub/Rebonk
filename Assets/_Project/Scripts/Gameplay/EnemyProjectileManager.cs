using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Owns the pool for enemy projectiles; ranged enemies call <see cref="Fire"/>.</summary>
    public class EnemyProjectileManager : MonoBehaviour
    {
        [SerializeField] private EnemyProjectile projectilePrefab;
        [SerializeField] private int prewarm = 20;
        [SerializeField] private int maxProjectiles = 400;

        private static EnemyProjectileManager _instance;
        private ComponentPool<EnemyProjectile> _pool;

        private void Awake()
        {
            _instance = this;
            _pool = new ComponentPool<EnemyProjectile>(projectilePrefab, PoolRoot.Transform, prewarm, maxProjectiles);
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public static void Fire(Vector2 position, Vector2 direction, float speed, float damage, float lifetime)
        {
            if (_instance == null)
                return;

            _instance._pool.Get().Init(position, direction, speed, damage, lifetime, _instance._pool.Release);
        }
    }
}
