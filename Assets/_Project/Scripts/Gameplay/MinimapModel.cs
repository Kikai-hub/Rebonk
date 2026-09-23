using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// The shared minimap texture for the current zone: terrain colour baked in once per chunk, and a
    /// "revealed" alpha channel stamped as the player walks (classic fog of war). One texture covers the
    /// whole bounded arena (see <see cref="ObstacleMap.ArenaHalfSize"/>), so both the small always-on
    /// thumbnail and the tap-to-open full map read from the very same data, just cropped differently.
    /// Reset every time a new zone is built (<see cref="MapGenerator.BuildZone"/>), which is also what
    /// the design calls for: a fresh, unexplored map for each new world/zone.
    /// </summary>
    public static class MinimapModel
    {
        /// <summary>How far around the player is revealed as they move (world units).</summary>
        public const float RevealRadius = 16f;

        private const int TexSize = 128;
        private const float MinRevealStepSqr = 1.5f * 1.5f; // skip re-stamping for tiny movements

        public static Texture2D Texture { get; private set; }
        public static float ArenaHalfSize { get; private set; }

        private static Color32[] _pixels;
        private static Vector2 _lastRevealPos;
        private static bool _everRevealed;

        /// <summary>Rebuilds a blank (unexplored) map sized to the given arena, pre-tinted with the world's base colour.</summary>
        public static void ResetForZone(float arenaHalfSize, Color baseColor)
        {
            ArenaHalfSize = Mathf.Max(1f, arenaHalfSize);

            if (Texture == null)
            {
                Texture = new Texture2D(TexSize, TexSize, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                _pixels = new Color32[TexSize * TexSize];
            }

            var c = (Color32)baseColor;
            c.a = 0; // hidden until revealed
            for (var i = 0; i < _pixels.Length; i++)
                _pixels[i] = c;

            Texture.SetPixels32(_pixels);
            Texture.Apply(false);
            _everRevealed = false;
        }

        /// <summary>Darkens a single world point (an obstacle's spot), leaving its revealed state untouched. Cheap: one texel.</summary>
        public static void PaintObstacle(Vector2 worldPos)
        {
            if (Texture == null)
                return;
            var (x, y) = ToTexel(worldPos);
            if (x < 0 || y < 0 || x >= TexSize || y >= TexSize)
                return;

            var i = y * TexSize + x;
            var c = _pixels[i];
            c.r = (byte)(c.r * 0.6f);
            c.g = (byte)(c.g * 0.6f);
            c.b = (byte)(c.b * 0.6f);
            _pixels[i] = c;
            Texture.SetPixel(x, y, c); // obstacles are painted one at a time as chunks load; no need to batch
        }

        /// <summary>Reveals a circle around <paramref name="worldPos"/>. Cheap to call every frame: no-ops for tiny moves.</summary>
        public static void Reveal(Vector2 worldPos)
        {
            if (Texture == null)
                return;
            if (_everRevealed && (worldPos - _lastRevealPos).sqrMagnitude < MinRevealStepSqr)
                return;
            _lastRevealPos = worldPos;
            _everRevealed = true;

            var (cx, cy) = ToTexel(worldPos);
            var texelRadius = Mathf.CeilToInt(RevealRadius / (ArenaHalfSize * 2f / TexSize)) + 1;
            var rSqr = texelRadius * texelRadius;

            var x0 = Mathf.Max(0, cx - texelRadius);
            var x1 = Mathf.Min(TexSize - 1, cx + texelRadius);
            var y0 = Mathf.Max(0, cy - texelRadius);
            var y1 = Mathf.Min(TexSize - 1, cy + texelRadius);

            for (var y = y0; y <= y1; y++)
            {
                var dy = y - cy;
                var rowStart = y * TexSize;
                for (var x = x0; x <= x1; x++)
                {
                    var dx = x - cx;
                    if (dx * dx + dy * dy > rSqr)
                        continue;
                    var i = rowStart + x;
                    var c = _pixels[i];
                    c.a = 255;
                    _pixels[i] = c;
                }
            }

            Texture.SetPixels32(x0, y0, x1 - x0 + 1, y1 - y0 + 1, Crop(x0, y0, x1, y1));
            Texture.Apply(false);
        }

        private static Color32[] Crop(int x0, int y0, int x1, int y1)
        {
            var w = x1 - x0 + 1;
            var h = y1 - y0 + 1;
            var result = new Color32[w * h];
            for (var y = 0; y < h; y++)
                System.Array.Copy(_pixels, (y0 + y) * TexSize + x0, result, y * w, w);
            return result;
        }

        /// <summary>True once the player has actually walked near this spot (its fog has been cleared).</summary>
        public static bool IsRevealed(Vector2 worldPos)
        {
            if (_pixels == null)
                return false;
            var (x, y) = ToTexel(worldPos);
            if (x < 0 || y < 0 || x >= TexSize || y >= TexSize)
                return false;
            return _pixels[y * TexSize + x].a > 0;
        }

        private static (int x, int y) ToTexel(Vector2 worldPos)
        {
            var u = (worldPos.x + ArenaHalfSize) / (ArenaHalfSize * 2f);
            var v = (worldPos.y + ArenaHalfSize) / (ArenaHalfSize * 2f);
            return (Mathf.FloorToInt(u * TexSize), Mathf.FloorToInt(v * TexSize));
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Texture = null;
            _pixels = null;
            ArenaHalfSize = 0f;
            _everRevealed = false;
        }
    }
}
