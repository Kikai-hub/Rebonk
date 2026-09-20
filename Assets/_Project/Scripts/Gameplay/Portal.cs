using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Appears where the boss was summoned once it dies. Walking into it leaves the zone (the only way forward).</summary>
    public class Portal : MonoBehaviour
    {
        [SerializeField] private float enterRadius = 1.3f;
        [SerializeField] private float spinDegreesPerSecond = 90f;
        [SerializeField] private float pulseAmount = 0.08f;

        private bool _used;
        private Vector3 _baseScale;

        private void Awake() => _baseScale = transform.localScale;

        private void OnEnable()
        {
            Waypoints.Portal = transform;
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.PortalOpen);
        }

        private void OnDisable()
        {
            if (Waypoints.Portal == transform)
                Waypoints.Portal = null;
        }

        private void Update()
        {
            transform.Rotate(0f, 0f, spinDegreesPerSecond * Time.deltaTime);
            transform.localScale = _baseScale * (1f + Mathf.Sin(Time.time * 4f) * pulseAmount);

            var player = PlayerController.Instance;
            if (_used || player == null || player.Health.IsDead || ZoneTimer.Instance == null)
                return;

            if (((Vector2)player.transform.position - (Vector2)transform.position).sqrMagnitude <= enterRadius * enterRadius)
            {
                _used = true;
                ZoneTimer.Instance.CompleteZone();
            }
        }
    }
}
