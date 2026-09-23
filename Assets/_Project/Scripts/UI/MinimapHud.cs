using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// Owns the small always-on minimap thumbnail and the tap-to-open full map. Tapping the thumbnail
    /// (or its own close button / the dimmed backdrop) toggles the full map, pausing the run and hiding
    /// the rest of the HUD while it is open, the same way the pause and level-up windows do.
    /// </summary>
    public class MinimapHud : MonoBehaviour
    {
        [SerializeField] private Button thumbnailButton;
        [SerializeField] private MinimapView thumbnailView;
        [Tooltip("World radius shown by the thumbnail: bigger than the game camera, so it reads as a wider look around the player.")]
        [SerializeField] private float thumbnailWorldRadius = 55f;

        [SerializeField] private GameObject expandedPanel;
        [SerializeField] private MinimapView expandedView;
        [SerializeField] private Button expandedCloseButton;
        [SerializeField] private Button expandedBackdropButton;

        private bool _open;

        private void Start()
        {
            expandedPanel.SetActive(false);
            thumbnailButton.onClick.AddListener(Open);
            expandedCloseButton.onClick.AddListener(Close);
            expandedBackdropButton.onClick.AddListener(Close);
        }

        private void OnDestroy()
        {
            if (_open)
                HudVisibility.Pop();
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player != null)
                thumbnailView.Refresh(player.transform.position, thumbnailWorldRadius);

            if (_open)
                expandedView.Refresh(Vector2.zero, Mathf.Max(1f, MinimapModel.ArenaHalfSize));
        }

        private void Open()
        {
            if (_open)
                return;
            _open = true;
            HudVisibility.Push();
            Time.timeScale = 0f;
            expandedPanel.SetActive(true);
        }

        private void Close()
        {
            if (!_open)
                return;
            _open = false;
            expandedPanel.SetActive(false);
            HudVisibility.Pop();
            Time.timeScale = 1f;
        }
    }
}
