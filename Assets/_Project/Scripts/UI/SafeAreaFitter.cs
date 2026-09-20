using UnityEngine;

namespace Rebonk.UI
{
    /// <summary>
    /// Keeps a full-screen RectTransform inside <see cref="Screen.safeArea"/> so nothing sits under a notch,
    /// rounded corners or the system gesture bar. Put it on the root of any panel that holds text or buttons.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Tooltip("Editor testing: pretend the screen has these insets in pixels (left, top, right, bottom). Zero = use the real safe area.")]
        [SerializeField] private Vector4 simulatedInsets;

        private RectTransform _rect;
        private Rect _lastArea;
        private Vector2Int _lastSize;

        private void Awake() => _rect = (RectTransform)transform;

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != _lastArea || Screen.width != _lastSize.x || Screen.height != _lastSize.y)
                Apply();
        }

        private void Apply()
        {
            if (_rect == null)
                _rect = (RectTransform)transform;

            var area = Screen.safeArea;
            if (simulatedInsets != Vector4.zero)
                area = Rect.MinMaxRect(simulatedInsets.x, simulatedInsets.w, Screen.width - simulatedInsets.z, Screen.height - simulatedInsets.y);

            _lastArea = Screen.safeArea;
            _lastSize = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            _rect.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            _rect.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
