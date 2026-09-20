using Rebonk.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rebonk.UI
{
    /// <summary>
    /// Reusable on-screen joystick. Put this on a full-area (transparent, raycast-target) zone.
    /// In floating mode the stick appears under the finger. Writes to <see cref="MoveInput.Joystick"/>.
    /// Both RectTransforms must be centered-anchored children of the zone (pivot 0.5).
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform knob;
        [SerializeField] private float radius = 90f;
        [SerializeField] private bool floating = true;
        [SerializeField, Range(0f, 1f)] private float deadZone = 0.1f;

        private RectTransform _zone;
        private Vector2 _homePosition;

        private void Awake()
        {
            _zone = (RectTransform)transform;
            _homePosition = background.anchoredPosition;
        }

        private void OnDisable() => ResetStick();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (floating &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_zone, eventData.position, eventData.pressEventCamera, out var local))
            {
                background.anchoredPosition = local;
            }

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out var local))
                return;

            var offset = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = offset;

            var value = offset / radius;
            MoveInput.Joystick = value.magnitude < deadZone ? Vector2.zero : value;
        }

        public void OnPointerUp(PointerEventData eventData) => ResetStick();

        private void ResetStick()
        {
            MoveInput.Joystick = Vector2.zero;
            if (knob != null)
                knob.anchoredPosition = Vector2.zero;
            if (background != null)
                background.anchoredPosition = _homePosition;
        }
    }
}
