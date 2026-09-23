using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// A totem standing on the map. Stay inside its radius for 5 seconds to activate it;
    /// leaving makes the progress drain. Activation offers 3 blessings (stat boosts for the rest of the run).
    /// </summary>
    public class Totem : MonoBehaviour
    {
        /// <summary>Raised when a totem finishes charging; the level-up screen answers with a blessing choice.</summary>
        public static event Action<Totem> Activated;

        public static readonly List<Totem> All = new List<Totem>(24);

        [SerializeField] private SpriteRenderer body;
        [Tooltip("Shown after activation. If empty, the body is tinted grey instead.")]
        [SerializeField] private Sprite usedSprite;
        [Tooltip("Ring sprite (radius 1 world unit at scale 1) showing the activation area and charge progress.")]
        [SerializeField] private SpriteRenderer ring;
        [SerializeField] private float activateRadius = 3.2f;
        [SerializeField] private float activateSeconds = 5f;
        [Tooltip("Progress drains this many times faster than it charges while the player is outside.")]
        [SerializeField] private float drainMultiplier = 2f;

        private static readonly Color IdleRing = new Color(0.1f, 0.6f, 0.85f, 0.55f);
        private static readonly Color ChargingRing = new Color(1f, 0.75f, 0.1f, 0.95f);

        private float _progress;
        private bool _done;

        public bool IsActivated => _done;
        public float Progress01 => activateSeconds > 0f ? _progress / activateSeconds : 0f;

        public static int TotalCount => All.Count;

        public static int UsedCount
        {
            get
            {
                var n = 0;
                foreach (var t in All)
                    if (t._done)
                        n++;
                return n;
            }
        }

        private void OnEnable() => All.Add(this);

        private void OnDisable() => All.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => All.Clear();

        private void Update()
        {
            if (_done)
                return;

            var player = PlayerController.Instance;
            var inside = player != null && !player.Health.IsDead &&
                         ((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude <= activateRadius * activateRadius;

            _progress = Mathf.Clamp(_progress + (inside ? Time.deltaTime : -Time.deltaTime * drainMultiplier), 0f, activateSeconds);
            UpdateVisuals(inside);

            if (_progress >= activateSeconds)
                Complete();
        }

        private void UpdateVisuals(bool inside)
        {
            if (ring == null)
                return;

            // The ring always shows the activation area; while charging a bright ring grows inside it.
            var full = activateRadius;
            var k = Progress01;
            ring.transform.localScale = new Vector3(full, full, 1f);
            ring.color = k > 0.001f ? Color.Lerp(IdleRing, ChargingRing, Mathf.Clamp01(k * 2f)) : IdleRing;
            if (k > 0.001f)
            {
                var pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.03f * k;
                ring.transform.localScale = new Vector3(full * pulse, full * pulse, 1f);
            }
        }

        private void Complete()
        {
            _done = true;
            if (body != null)
            {
                if (usedSprite != null)
                    body.sprite = usedSprite;
                else
                    body.color = new Color(0.45f, 0.45f, 0.5f, 1f);
            }
            if (ring != null)
                ring.enabled = false;

            Activated?.Invoke(this);
        }
    }
}
