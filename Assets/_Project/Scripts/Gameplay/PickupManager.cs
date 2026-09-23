using System;
using System.Collections;
using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Rare item drops (magnet / speed boost / hot egg) and the boss-death sweep:
    /// when a boss dies every regular enemy dies too and all gems on the map fly to the player.
    /// </summary>
    public class PickupManager : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public PickupKind kind;
            public Sprite sprite;
            [Min(0f)] public float weight = 1f;
        }

        [SerializeField] private Pickup pickupPrefab;
        [SerializeField] private Entry[] items;

        [Header("Drop rules")]
        [Tooltip("Chance per regular enemy kill that some item drops.")]
        [Range(0f, 0.2f)] [SerializeField] private float dropChance = 0.01f;
        [Tooltip("No new drop sooner than this after the previous one.")]
        [SerializeField] private float minSecondsBetweenDrops = 8f;
        [Tooltip("No new drops while this many items already lie on the map.")]
        [SerializeField] private int maxOnMap = 6;

        [Header("Speed boost")]
        [SerializeField] private float speedMultiplier = 3f;
        [SerializeField] private float speedSeconds = 20f;

        [Header("Hot egg")]
        [Tooltip("Every regular enemy within this radius of the player dies (about one screen).")]
        [SerializeField] private float eggRadius = 12f;
        [Tooltip("A boss caught in the blast loses this fraction of its max HP; the egg never kills a boss.")]
        [Range(0f, 1f)] [SerializeField] private float eggBossDamageFraction = 0.5f;
        [SerializeField] private Sprite blastSprite;
        [SerializeField] private Color blastColor = new Color(1f, 0.55f, 0.15f, 0.9f);

        private ComponentPool<Pickup> _pool;
        private SpriteRenderer _blast;
        private Coroutine _blastRoutine;
        private float _nextDropTime;
        private bool _suppressDrops;

        private void Awake()
        {
            _pool = new ComponentPool<Pickup>(pickupPrefab, transform, 4, 32);

            var go = new GameObject("EggBlast");
            go.transform.SetParent(transform, false);
            _blast = go.AddComponent<SpriteRenderer>();
            _blast.sprite = blastSprite;
            _blast.sortingOrder = 20;
            go.SetActive(false);
        }

        private void OnEnable()
        {
            Enemy.Killed += OnEnemyKilled;
            BossController.Defeated += OnBossDefeated;
        }

        private void Start()
        {
            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            Enemy.Killed -= OnEnemyKilled;
            BossController.Defeated -= OnBossDefeated;
            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(ZonePhase phase)
        {
            // A new zone is a new map: items left behind disappear like gems do.
            if (phase == ZonePhase.Cleared)
                DespawnAll();
        }

        private void DespawnAll()
        {
            for (var i = Pickup.Active.Count - 1; i >= 0; i--)
                _pool.Release(Pickup.Active[i]);
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            if (_suppressDrops || enemy.IsBoss || Time.time < _nextDropTime || Pickup.Active.Count >= maxOnMap)
                return;
            if (UnityEngine.Random.value >= dropChance)
                return;

            var entry = PickRandom();
            if (entry == null)
                return;

            _nextDropTime = Time.time + minSecondsBetweenDrops;
            Spawn(entry, enemy.transform.position);
        }

        /// <summary>Drops a specific item (debug / tests).</summary>
        public void Spawn(PickupKind kind, Vector2 position)
        {
            foreach (var e in items)
                if (e.kind == kind)
                {
                    Spawn(e, position);
                    return;
                }
        }

        private void Spawn(Entry entry, Vector2 position)
        {
            _pool.Get().Init(entry.kind, entry.sprite, position, OnCollected);
        }

        private Entry PickRandom()
        {
            var total = 0f;
            foreach (var e in items)
                total += e.weight;
            if (total <= 0f)
                return null;

            var roll = UnityEngine.Random.value * total;
            foreach (var e in items)
            {
                roll -= e.weight;
                if (roll <= 0f)
                    return e;
            }
            return items[items.Length - 1];
        }

        private void OnCollected(Pickup pickup)
        {
            var kind = pickup.Kind;
            _pool.Release(pickup);

            var player = PlayerController.Instance;
            if (player == null)
                return;

            switch (kind)
            {
                case PickupKind.Magnet:
                    Sound.Play(SfxId.GemPickup);
                    XpGem.AttractAll();
                    break;
                case PickupKind.SpeedBoost:
                    Sound.Play(SfxId.LevelUp);
                    player.ApplySpeedBoost(speedMultiplier, speedSeconds);
                    break;
                case PickupKind.HotEgg:
                    Sound.Play(SfxId.WeaponArea);
                    Explode(player.transform.position);
                    break;
            }
        }

        private void Explode(Vector2 center)
        {
            _suppressDrops = true;
            try
            {
                var radiusSqr = eggRadius * eggRadius;
                var list = Enemy.Active;
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (i >= list.Count)
                        continue;
                    var e = list[i];
                    if (e.Health.IsDead || ((Vector2)e.transform.position - center).sqrMagnitude > radiusSqr)
                        continue;

                    if (e.IsBoss)
                    {
                        var damage = Mathf.Min(e.Health.Max * eggBossDamageFraction, e.Health.Current - 1f);
                        e.Health.TakeDamage(damage);
                    }
                    else
                    {
                        e.Health.TakeDamage(e.Health.Current + 1f);
                    }
                }
            }
            finally
            {
                _suppressDrops = false;
            }

            if (blastSprite != null)
            {
                if (_blastRoutine != null)
                    StopCoroutine(_blastRoutine);
                _blastRoutine = StartCoroutine(BlastEffect(center));
            }
        }

        private IEnumerator BlastEffect(Vector2 center)
        {
            var go = _blast.gameObject;
            go.transform.position = center;
            go.SetActive(true);
            var spriteRadius = Mathf.Max(0.01f, blastSprite.bounds.extents.x);

            const float duration = 0.4f;
            for (var t = 0f; t < duration; t += Time.deltaTime)
            {
                var k = t / duration;
                var scale = Mathf.Lerp(0.2f, 1f, 1f - (1f - k) * (1f - k)) * eggRadius / spriteRadius;
                go.transform.localScale = new Vector3(scale, scale, 1f);
                var c = blastColor;
                c.a *= 1f - k;
                _blast.color = c;
                yield return null;
            }
            go.SetActive(false);
            _blastRoutine = null;
        }

        private void OnBossDefeated(BossController boss)
        {
            _suppressDrops = true;
            try
            {
                Enemy.KillAllExceptBosses();
            }
            finally
            {
                _suppressDrops = false;
            }
            EnemyProjectile.DespawnAll();
            XpGem.AttractAll();
        }
    }
}
