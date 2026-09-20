using UnityEngine;

namespace Rebonk.Gameplay
{
    public static class EnemyQuery
    {
        /// <summary>The nearest live enemy within <paramref name="range"/>.</summary>
        public static bool TryGetNearestEnemy(Vector2 origin, float range, out Enemy nearest)
        {
            var best = range * range;
            nearest = null;

            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var sq = ((Vector2)list[i].transform.position - origin).sqrMagnitude;
                if (sq > best)
                    continue;
                best = sq;
                nearest = list[i];
            }

            return nearest != null;
        }

        /// <summary>Direction (normalized) to the nearest live enemy within <paramref name="range"/>.</summary>
        public static bool TryGetNearest(Vector2 origin, float range, out Vector2 direction)
        {
            var best = range * range;
            var found = false;
            direction = Vector2.right;

            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var to = (Vector2)list[i].transform.position - origin;
                var sq = to.sqrMagnitude;
                if (sq > best)
                    continue;
                best = sq;
                direction = to;
                found = true;
            }

            if (found && direction.sqrMagnitude > 0.0001f)
                direction.Normalize();
            else
                direction = Vector2.right;

            return found;
        }
    }
}
