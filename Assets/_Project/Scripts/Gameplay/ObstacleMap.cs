using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Solid circles the player cannot walk through, bucketed per map chunk for O(1) lookups.
    /// Enemies ignore obstacles on purpose (absurd-comedy "phasing"), which keeps pathing free.
    /// </summary>
    public static class ObstacleMap
    {
        public struct Circle
        {
            public Vector2 Center;
            public float Radius;
        }

        private static readonly Dictionary<Vector2Int, List<Circle>> Chunks = new Dictionary<Vector2Int, List<Circle>>();
        private static int _chunkSize = 16;

        public static void Configure(int chunkSize) => _chunkSize = Mathf.Max(1, chunkSize);

        public static void Set(Vector2Int chunk, List<Circle> circles) => Chunks[chunk] = circles;

        public static void Remove(Vector2Int chunk) => Chunks.Remove(chunk);

        public static void Clear() => Chunks.Clear();

        /// <summary>Pushes <paramref name="position"/> out of every overlapping obstacle so the mover slides along them.</summary>
        public static Vector2 Resolve(Vector2 position, float radius)
        {
            var cx = Mathf.FloorToInt(position.x / _chunkSize);
            var cy = Mathf.FloorToInt(position.y / _chunkSize);

            for (var pass = 0; pass < 2; pass++)
            {
                for (var dx = -1; dx <= 1; dx++)
                {
                    for (var dy = -1; dy <= 1; dy++)
                    {
                        if (!Chunks.TryGetValue(new Vector2Int(cx + dx, cy + dy), out var list))
                            continue;

                        for (var i = 0; i < list.Count; i++)
                        {
                            var c = list[i];
                            var delta = position - c.Center;
                            var min = radius + c.Radius;
                            var sqr = delta.sqrMagnitude;
                            if (sqr >= min * min)
                                continue;

                            var dist = Mathf.Sqrt(sqr);
                            position = dist < 0.0001f ? c.Center + Vector2.up * min : c.Center + delta / dist * min;
                        }
                    }
                }
            }

            return position;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Chunks.Clear();
            _chunkSize = 16;
        }
    }
}
