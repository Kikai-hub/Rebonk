using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Rebonk.Core
{
    /// <summary>
    /// Prefab pool used for every runtime-spawned entity (enemies, projectiles, gems, VFX).
    /// Instances are deactivated on release and activated on get; never Instantiate/Destroy in gameplay code.
    /// </summary>
    public class ComponentPool<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;

        public ComponentPool(T prefab, Transform parent, int prewarm = 0, int maxSize = 1000)
        {
            _pool = new ObjectPool<T>(
                createFunc: () => Object.Instantiate(prefab, parent),
                actionOnGet: c => c.gameObject.SetActive(true),
                actionOnRelease: c => c.gameObject.SetActive(false),
                actionOnDestroy: c => Object.Destroy(c.gameObject),
                collectionCheck: false,
                defaultCapacity: Mathf.Max(prewarm, 10),
                maxSize: maxSize);

            if (prewarm > 0)
            {
                var temp = new List<T>(prewarm);
                for (var i = 0; i < prewarm; i++)
                    temp.Add(_pool.Get());
                for (var i = 0; i < temp.Count; i++)
                    _pool.Release(temp[i]);
            }
        }

        public T Get() => _pool.Get();

        public void Release(T instance) => _pool.Release(instance);
    }
}
