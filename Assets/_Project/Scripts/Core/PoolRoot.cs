using UnityEngine;

namespace Rebonk.Core
{
    /// <summary>World-space parent for pooled objects that must not move with their owner (projectiles, gems...).</summary>
    public static class PoolRoot
    {
        private static Transform _root;

        public static Transform Transform
        {
            get
            {
                if (_root == null)
                    _root = new GameObject("Pools").transform;
                return _root;
            }
        }
    }
}
