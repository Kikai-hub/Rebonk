using UnityEngine;

namespace Rebonk.UI
{
    /// <summary>
    /// Hides the in-run HUD while a full-screen window (level-up, totem blessing, pause, game over) is open,
    /// so HUD text never shows through the dimmed overlay and overlaps the window's own text.
    /// Windows call <see cref="Push"/> when they open and <see cref="Pop"/> when they close.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class HudVisibility : MonoBehaviour
    {
        private static HudVisibility _instance;
        private static int _openWindows;

        private CanvasGroup _group;

        private void Awake()
        {
            _instance = this;
            _openWindows = 0;
            _group = GetComponent<CanvasGroup>();
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _openWindows = 0;
            }
        }

        public static void Push()
        {
            _openWindows++;
            Apply();
        }

        public static void Pop()
        {
            _openWindows = Mathf.Max(0, _openWindows - 1);
            Apply();
        }

        private static void Apply()
        {
            if (_instance == null)
                return;

            var visible = _openWindows == 0;
            _instance._group.alpha = visible ? 1f : 0f;
            _instance._group.blocksRaycasts = visible;
            _instance._group.interactable = visible;
        }
    }
}
