using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Streams an endless procedural map in square chunks around the player.
    /// Generation is deterministic per (seed, chunk), so a chunk looks the same when it is reloaded.
    ///
    /// Reachability guarantee (altar / portal): obstacles are kept at least <c>obstacleGap</c> apart
    /// (edge to edge) inside a chunk and at least a chunk-margin away from chunk borders, so any two
    /// obstacles anywhere on the map are separated by a gap the player can walk through. Obstacle
    /// clusters therefore can never wall anything in. The altar additionally gets a cleared circle.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        public static MapGenerator Instance { get; private set; }

        [SerializeField] private Tilemap groundMap;
        [SerializeField] private Altar altar;
        [SerializeField] private int chunkSize = 16;
        [Tooltip("Chunks kept loaded around the player in every direction.")]
        [SerializeField] private int loadRadius = 3;

        [Header("Placement")]
        [SerializeField] private float startSafeRadius = 6f;
        [SerializeField] private float altarClearRadius = 4f;
        [Tooltip("The altar lands at a random angle and a random distance from the start within this range.")]
        [SerializeField] private Vector2 altarDistance = new Vector2(28f, 85f);

        [Header("Totems")]
        [SerializeField] private Totem totemPrefab;
        [Tooltip("How many totems each zone gets (inclusive range).")]
        [SerializeField] private Vector2Int totemCount = new Vector2Int(15, 20);
        [Tooltip("Totems are scattered between these distances from the start.")]
        [SerializeField] private Vector2 totemDistance = new Vector2(12f, 95f);
        [SerializeField] private float totemSpacing = 14f;
        [SerializeField] private float totemClearRadius = 3f;

        private struct ObstacleData
        {
            public Vector2 Position;
            public WorldProp Prop;
        }

        private sealed class Chunk
        {
            public readonly List<GameObject> Objects = new List<GameObject>();
            public readonly List<GameObject> ObstacleObjects = new List<GameObject>();
            public readonly List<ObstacleMap.Circle> Circles = new List<ObstacleMap.Circle>();
        }

        private readonly Dictionary<Vector2Int, Chunk> _chunks = new Dictionary<Vector2Int, Chunk>();
        private readonly List<Vector2Int> _toUnload = new List<Vector2Int>();
        private readonly List<Vector2Int> _loadQueue = new List<Vector2Int>();
        private readonly List<KeyValuePair<Vector2, float>> _safeZones = new List<KeyValuePair<Vector2, float>>();
        private readonly Stack<GameObject> _propPool = new Stack<GameObject>();
        private readonly List<Totem> _totems = new List<Totem>();

        private WorldConfig _world;
        private int _seed;
        private Vector2Int _lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
        private Tile[] _groundTiles;
        private Tile[] _patchTiles;
        private Transform _propRoot;
        private float _noiseOffsetX;
        private float _noiseOffsetY;

        public int ChunkCount => _chunks.Count;
        public int ObstacleCount
        {
            get
            {
                var n = 0;
                foreach (var c in _chunks.Values) n += c.ObstacleObjects.Count;
                return n;
            }
        }

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            ObstacleMap.Clear();
        }

        private void Start()
        {
            _propRoot = new GameObject("MapProps").transform;
            _world = RunSettings.World;
            ObstacleMap.Configure(chunkSize);
            _groundTiles = MakeTiles(_world.groundTiles);
            _patchTiles = MakeTiles(_world.patchTiles);

            BuildZone(1);

            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged += OnPhaseChanged;
        }

        private static Tile[] MakeTiles(Sprite[] sprites)
        {
            if (sprites == null)
                return new Tile[0];

            var tiles = new Tile[sprites.Length];
            for (var i = 0; i < sprites.Length; i++)
            {
                var t = ScriptableObject.CreateInstance<Tile>();
                t.sprite = sprites[i];
                t.colliderType = Tile.ColliderType.None;
                tiles[i] = t;
            }
            return tiles;
        }

        private void OnPhaseChanged(ZonePhase phase)
        {
            // A new zone is a brand new map: the player starts again at the origin.
            if (phase == ZonePhase.Zone && ZoneTimer.Instance.ZoneLevel > 1)
            {
                BuildZone(ZoneTimer.Instance.ZoneLevel);
                var player = PlayerController.Instance;
                if (player != null)
                    player.transform.position = Vector3.zero;
                XpGem.DespawnAll();
                EnemyProjectile.DespawnAll();
                _lastPlayerChunk = new Vector2Int(int.MinValue, int.MinValue);
            }
        }

        private void BuildZone(int zoneLevel)
        {
            UnloadAll();
            _seed = RunSettings.Seed + zoneLevel * 7919;
            var rng = new Random(_seed);
            _noiseOffsetX = (float)rng.NextDouble() * 1000f;
            _noiseOffsetY = (float)rng.NextDouble() * 1000f;

            _safeZones.Clear();
            _safeZones.Add(new KeyValuePair<Vector2, float>(Vector2.zero, startSafeRadius));

            var altarPosition = PickAltarPosition(rng);
            _safeZones.Add(new KeyValuePair<Vector2, float>(altarPosition, altarClearRadius));
            if (altar != null)
                altar.PlaceAt(altarPosition);

            PlaceTotems(rng, altarPosition);
            RefreshChunks(Vector2Int.zero, true);
        }

        private void PlaceTotems(Random rng, Vector2 altarPosition)
        {
            foreach (var old in _totems)
            {
                if (old == null) continue;
                old.gameObject.SetActive(false);
                Destroy(old.gameObject);
            }
            _totems.Clear();

            if (totemPrefab == null)
                return;

            var wanted = rng.Next(totemCount.x, totemCount.y + 1);
            var positions = new List<Vector2>();
            var spacingSqr = totemSpacing * totemSpacing;

            for (var attempt = 0; attempt < wanted * 120 && positions.Count < wanted; attempt++)
            {
                var angle = (float)rng.NextDouble() * Mathf.PI * 2f;
                var dist = Mathf.Lerp(totemDistance.x, totemDistance.y, Mathf.Sqrt((float)rng.NextDouble())); // sqrt = even spread over the area
                var pos = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

                var ok = (pos - altarPosition).sqrMagnitude >= spacingSqr;
                for (var i = 0; ok && i < positions.Count; i++)
                    ok = (positions[i] - pos).sqrMagnitude >= spacingSqr;
                if (ok)
                    positions.Add(pos);
            }

            foreach (var pos in positions)
            {
                _safeZones.Add(new KeyValuePair<Vector2, float>(pos, totemClearRadius)); // keep obstacles off the totem
                var totem = Instantiate(totemPrefab, pos, Quaternion.identity);
                _totems.Add(totem);
            }
        }

        private Vector2 PickAltarPosition(Random rng)
        {
            var angle = (float)rng.NextDouble() * Mathf.PI * 2f;
            var dist = Mathf.Lerp(altarDistance.x, altarDistance.y, (float)rng.NextDouble());
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null)
                return;

            var p = player.transform.position;
            var chunk = new Vector2Int(Mathf.FloorToInt(p.x / chunkSize), Mathf.FloorToInt(p.y / chunkSize));
            if (chunk != _lastPlayerChunk)
            {
                _lastPlayerChunk = chunk;
                RefreshChunks(chunk, false);
            }

            StreamChunks();
        }

        /// <summary>
        /// Works out which chunks around <paramref name="center"/> are missing or too far away. With <paramref name="immediate"/>
        /// (a fresh zone) everything is done at once, otherwise <see cref="StreamChunks"/> handles one chunk per frame so that
        /// crossing a chunk border never causes a frame hitch on a slow phone (the window is much larger than the screen).
        /// </summary>
        private void RefreshChunks(Vector2Int center, bool immediate)
        {
            _loadQueue.Clear();
            for (var x = -loadRadius; x <= loadRadius; x++)
            {
                for (var y = -loadRadius; y <= loadRadius; y++)
                {
                    var c = new Vector2Int(center.x + x, center.y + y);
                    if (!_chunks.ContainsKey(c))
                        _loadQueue.Add(c);
                }
            }

            // nearest first, so the chunks the player is about to see are ready before the far ones
            _loadQueue.Sort((a, b) => (a - center).sqrMagnitude.CompareTo((b - center).sqrMagnitude));

            _toUnload.Clear();
            foreach (var c in _chunks.Keys)
            {
                if (Mathf.Abs(c.x - center.x) > loadRadius + 1 || Mathf.Abs(c.y - center.y) > loadRadius + 1)
                    _toUnload.Add(c);
            }

            if (!immediate)
                return;

            foreach (var c in _loadQueue)
                LoadChunk(c);
            _loadQueue.Clear();
            foreach (var c in _toUnload)
                UnloadChunk(c);
            _toUnload.Clear();
        }

        private void StreamChunks()
        {
            if (_loadQueue.Count > 0)
            {
                var c = _loadQueue[0];
                _loadQueue.RemoveAt(0);
                if (!_chunks.ContainsKey(c))
                    LoadChunk(c);
            }
            else if (_toUnload.Count > 0)
            {
                var last = _toUnload.Count - 1;
                UnloadChunk(_toUnload[last]);
                _toUnload.RemoveAt(last);
            }
        }

        // ---------- chunk generation ----------

        private static int Hash(int seed, int x, int y, int salt)
        {
            unchecked
            {
                var h = seed;
                h = h * 73856093 ^ x * 19349663;
                h = h * 83492791 ^ y * 39916801;
                return h ^ (salt * 668265263);
            }
        }

        private void LoadChunk(Vector2Int c)
        {
            var chunk = new Chunk();
            _chunks[c] = chunk;

            PaintGround(c);

            var originX = c.x * chunkSize;
            var originY = c.y * chunkSize;

            // obstacles (layout is generated ignoring safe zones so it is stable; zones only remove entries)
            foreach (var o in GenerateObstacles(c))
            {
                if (InSafeZone(o.Position, o.Prop.collisionRadius))
                    continue;

                var go = SpawnProp(o.Prop, o.Position, 3);
                chunk.Objects.Add(go);
                chunk.ObstacleObjects.Add(go);
                chunk.Circles.Add(new ObstacleMap.Circle { Center = o.Position, Radius = o.Prop.collisionRadius });
            }
            ObstacleMap.Set(c, chunk.Circles);

            // decorations
            if (_world.decorations != null && _world.decorations.Length > 0)
            {
                var rng = new Random(Hash(_seed, c.x, c.y, 2));
                for (var i = 0; i < _world.decorationsPerChunk; i++)
                {
                    var pos = new Vector2(originX + (float)rng.NextDouble() * chunkSize, originY + (float)rng.NextDouble() * chunkSize);
                    var prop = PickWeighted(_world.decorations, rng);
                    if (prop == null || InSafeZone(pos, 0f))
                        continue;
                    chunk.Objects.Add(SpawnProp(prop, pos, 1));
                }
            }
        }

        private List<ObstacleData> GenerateObstacles(Vector2Int c)
        {
            var result = new List<ObstacleData>();
            if (_world.obstacles == null || _world.obstacles.Length == 0)
                return result;

            var rng = new Random(Hash(_seed, c.x, c.y, 1));
            var maxRadius = 0f;
            foreach (var p in _world.obstacles)
                maxRadius = Mathf.Max(maxRadius, p.collisionRadius);

            // Stay this far from the chunk border so obstacles of neighbouring chunks can never touch.
            var margin = maxRadius + _world.obstacleGap * 0.5f;
            var span = chunkSize - margin * 2f;
            if (span <= 0f)
                return result;

            var originX = c.x * chunkSize + margin;
            var originY = c.y * chunkSize + margin;

            for (var attempt = 0; attempt < _world.obstacleAttemptsPerChunk; attempt++)
            {
                var prop = PickWeighted(_world.obstacles, rng);
                var pos = new Vector2(originX + (float)rng.NextDouble() * span, originY + (float)rng.NextDouble() * span);
                if (prop == null)
                    continue;

                var ok = true;
                foreach (var other in result)
                {
                    var need = prop.collisionRadius + other.Prop.collisionRadius + _world.obstacleGap;
                    if ((other.Position - pos).sqrMagnitude < need * need)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok)
                    result.Add(new ObstacleData { Position = pos, Prop = prop });
            }

            return result;
        }

        private static WorldProp PickWeighted(WorldProp[] props, Random rng)
        {
            var total = 0f;
            foreach (var p in props) total += p.weight;
            if (total <= 0f)
                return null;

            var roll = (float)rng.NextDouble() * total;
            foreach (var p in props)
            {
                roll -= p.weight;
                if (roll <= 0f)
                    return p;
            }
            return props[props.Length - 1];
        }

        private void PaintGround(Vector2Int c)
        {
            if (groundMap == null || _groundTiles.Length == 0)
                return;

            var rng = new Random(Hash(_seed, c.x, c.y, 3));
            var tiles = new TileBase[chunkSize * chunkSize];
            var baseX = c.x * chunkSize;
            var baseY = c.y * chunkSize;

            for (var y = 0; y < chunkSize; y++)
            {
                for (var x = 0; x < chunkSize; x++)
                {
                    var wx = baseX + x;
                    var wy = baseY + y;
                    var noise = Mathf.PerlinNoise((wx + _noiseOffsetX) * _world.patchNoiseScale, (wy + _noiseOffsetY) * _world.patchNoiseScale);
                    var set = noise > _world.patchThreshold && _patchTiles.Length > 0 ? _patchTiles : _groundTiles;
                    tiles[y * chunkSize + x] = set[rng.Next(set.Length)];
                }
            }

            groundMap.SetTilesBlock(new BoundsInt(baseX, baseY, 0, chunkSize, chunkSize, 1), tiles);
        }

        private bool InSafeZone(Vector2 pos, float radius)
        {
            foreach (var z in _safeZones)
            {
                var r = z.Value + radius;
                if ((pos - z.Key).sqrMagnitude < r * r)
                    return true;
            }
            return false;
        }

        // ---------- pooling ----------

        private GameObject SpawnProp(WorldProp prop, Vector2 position, int sortingOrder)
        {
            GameObject go;
            SpriteRenderer sr;
            if (_propPool.Count > 0)
            {
                go = _propPool.Pop();
                sr = go.GetComponent<SpriteRenderer>();
                go.SetActive(true);
            }
            else
            {
                go = new GameObject("Prop");
                go.transform.SetParent(_propRoot, false);
                sr = go.AddComponent<SpriteRenderer>();
            }

            go.transform.position = position;
            go.transform.localScale = Vector3.one * prop.scale;
            sr.sprite = prop.sprite;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        private void UnloadChunk(Vector2Int c)
        {
            if (!_chunks.TryGetValue(c, out var chunk))
                return;

            foreach (var go in chunk.Objects)
            {
                go.SetActive(false);
                _propPool.Push(go);
            }

            ObstacleMap.Remove(c);
            _chunks.Remove(c);

            if (groundMap != null)
                groundMap.SetTilesBlock(new BoundsInt(c.x * chunkSize, c.y * chunkSize, 0, chunkSize, chunkSize, 1), new TileBase[chunkSize * chunkSize]);
        }

        private void UnloadAll()
        {
            var keys = new List<Vector2Int>(_chunks.Keys);
            foreach (var c in keys)
                UnloadChunk(c);
            ObstacleMap.Clear();
            if (groundMap != null)
                groundMap.ClearAllTiles();
        }
    }
}
