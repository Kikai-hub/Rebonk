using UnityEngine;
using UnityEngine.InputSystem;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Boss altar. Walk up to it (a pulsing ring shows you are in range) and TAP / click the altar to summon the zone boss.
    /// One use per zone. When the boss dies a portal appears where it was summoned.
    /// </summary>
    public class Altar : MonoBehaviour
    {
        private enum State { Idle, BossActive, Consumed }

        [SerializeField] private Enemy bossPrefab;
        [SerializeField] private EnemyData bossData;
        [SerializeField] private Portal portalPrefab;
        [SerializeField] private DifficultyConfig config;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private SpriteRenderer body;
        [Tooltip("Ring sprite (radius 1 unit at scale 1) pulsing around the altar while the player is close enough to use it.")]
        [SerializeField] private SpriteRenderer promptRing;

        [Header("Interaction")]
        [Tooltip("The player must be this close for a tap on the altar to work.")]
        [SerializeField] private float interactRadius = 4f;
        [Tooltip("How close (world units) to the altar a tap has to land. Generous so fingers do not miss.")]
        [SerializeField] private float tapRadius = 1.6f;

        private State _state = State.Idle;
        private Enemy _boss;
        private Portal _portal;
        private bool _discovered;

        public bool IsUsable => _state == State.Idle;

        /// <summary>Called by the map generator: the altar stands wherever the generated map puts it.</summary>
        public void PlaceAt(Vector2 position) => transform.position = position;

        private void OnEnable()
        {
            Enemy.Killed += OnEnemyKilled;
            SetWaypoint();
        }

        private void OnDisable()
        {
            Enemy.Killed -= OnEnemyKilled;
            if (Waypoints.Altar == transform)
                Waypoints.Altar = null;
        }

        private void Start()
        {
            if (promptRing != null)
                promptRing.enabled = false;

            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged += OnPhaseChanged;
        }

        private void OnDestroy()
        {
            if (ZoneTimer.Instance != null)
                ZoneTimer.Instance.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(ZonePhase phase)
        {
            if (phase != ZonePhase.Zone)
                return;

            // New zone: the MapGenerator has already moved the altar to its new spot; reset state, remove the old portal.
            if (_portal != null)
                Destroy(_portal.gameObject);
            _portal = null;
            _boss = null;
            _state = State.Idle;
            _discovered = false;
            SetVisuals();
            SetWaypoint();
        }

        private void Update()
        {
            if (!_discovered && _state == State.Idle)
                CheckDiscovered();

            var player = PlayerController.Instance;
            var inRange = _state == State.Idle && player != null && !player.Health.IsDead &&
                          ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude <= interactRadius * interactRadius;

            if (promptRing != null)
            {
                promptRing.enabled = inRange;
                if (inRange)
                {
                    var s = tapRadius * (1f + 0.15f * Mathf.Sin(Time.time * 6f));
                    promptRing.transform.localScale = new Vector3(s, s, 1f);
                }
            }

            if (inRange && WasTapped())
                Activate();
        }

        /// <summary>The HUD arrow only starts pointing at the altar once the player has actually seen it on screen.</summary>
        private void CheckDiscovered()
        {
            var cam = viewCamera != null ? viewCamera : Camera.main;
            if (cam == null)
                return;

            var v = cam.WorldToViewportPoint(transform.position);
            if (v.z > 0f && v.x >= 0f && v.x <= 1f && v.y >= 0f && v.y <= 1f)
            {
                _discovered = true;
                SetWaypoint();
            }
        }

        /// <summary>True when a press that began this frame landed on the altar (touch or mouse).</summary>
        private bool WasTapped()
        {
            var cam = viewCamera != null ? viewCamera : Camera.main;
            if (cam == null)
                return false;

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && HitsAltar(cam, mouse.position.ReadValue()))
                return true;

            var touch = Touchscreen.current;
            if (touch != null)
            {
                foreach (var t in touch.touches)
                {
                    if (t.press.wasPressedThisFrame && HitsAltar(cam, t.position.ReadValue()))
                        return true;
                }
            }

            return false;
        }

        private bool HitsAltar(Camera cam, Vector2 screenPosition)
        {
            Vector2 world = cam.ScreenToWorldPoint(screenPosition);
            return (world - (Vector2)transform.position).sqrMagnitude <= tapRadius * tapRadius;
        }

        private void Activate()
        {
            _state = State.BossActive;
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.AltarActivate);
            if (promptRing != null)
                promptRing.enabled = false;

            var minutes = RunClock.Instance != null ? RunClock.Instance.Minutes : 0f;
            var zone = ZoneTimer.Instance != null ? ZoneTimer.Instance.ZoneLevel : 1;
            var scale = config != null ? config.EvaluateScale(minutes, zone) : EnemyScale.One;

            // The selected world may bring its own boss; the inspector fields are the fallback.
            var world = RunSettings.World;
            var prefab = world != null && world.bossPrefab != null ? world.bossPrefab : bossPrefab;
            var data = world != null && world.bossData != null ? world.bossData : bossData;
            if (world != null)
            {
                scale.Hp *= world.hpMultiplier;
                scale.Damage *= world.damageMultiplier;
            }

            _boss = Instantiate(prefab, transform.position, Quaternion.identity);
            _boss.Init(data, e => Destroy(e.gameObject), scale);

            SetVisuals();
            SetWaypoint();
        }

        /// <summary>Debug / test hook: summon the boss without tapping.</summary>
        [ContextMenu("Activate (debug)")]
        public void DebugActivate()
        {
            if (_state == State.Idle)
                Activate();
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            if (_state != State.BossActive || enemy != _boss)
                return;

            _state = State.Consumed;
            _portal = Instantiate(portalPrefab, transform.position, Quaternion.identity); // where the boss was summoned
            SetVisuals();
            SetWaypoint();
        }

        private void SetVisuals()
        {
            if (body != null)
                body.color = _state == State.Idle ? Color.white : new Color(0.45f, 0.45f, 0.5f, 1f);
        }

        private void SetWaypoint()
        {
            // The HUD arrow points at the altar only once it has been seen, and only while it can still be used.
            Waypoints.Altar = _state == State.Idle && _discovered ? transform : null;
        }
    }
}
