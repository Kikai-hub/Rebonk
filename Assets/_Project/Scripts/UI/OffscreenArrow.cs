using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>HUD arrow on the edge of the safe screen area pointing at the altar or portal while it is off screen.</summary>
    public class OffscreenArrow : MonoBehaviour
    {
        public enum TargetKind { Altar, Portal }

        [SerializeField] private TargetKind kind;
        [SerializeField] private Image image;
        [SerializeField] private float edgeMargin = 70f;
        [SerializeField] private Camera viewCamera;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            image.enabled = false;
        }

        private void LateUpdate()
        {
            var target = kind == TargetKind.Altar ? Waypoints.Altar : Waypoints.Portal;
            if (target == null || viewCamera == null)
            {
                image.enabled = false;
                return;
            }

            // Use the safe area so the arrow is never hidden under a notch or the gesture bar.
            var safe = Screen.safeArea;
            Vector2 screen = viewCamera.WorldToScreenPoint(target.position);
            var center = safe.center;
            var half = safe.size * 0.5f - Vector2.one * edgeMargin;

            var offset = screen - center;
            if (Mathf.Abs(offset.x) <= half.x && Mathf.Abs(offset.y) <= half.y)
            {
                image.enabled = false; // target is on screen
                return;
            }

            // Clamp along the ray from the screen center to the target so the arrow sits on the edge rectangle.
            var k = Mathf.Min(half.x / Mathf.Max(0.001f, Mathf.Abs(offset.x)), half.y / Mathf.Max(0.001f, Mathf.Abs(offset.y)));
            _rect.position = center + offset * k;
            _rect.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg);
            image.enabled = true;
        }
    }
}
