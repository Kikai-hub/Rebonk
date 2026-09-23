using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>A list row for quests and achievements: icon, title, description, progress, and a done state.</summary>
    public class InfoRow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text progressText;
        [Header("Optional icon in a frame (frame sprite switches when done)")]
        [SerializeField] private Image icon;
        [SerializeField] private Image iconFrame;
        [SerializeField] private Sprite framePending;
        [SerializeField] private Sprite frameDone;
        [SerializeField] private Color progressPendingColor = Color.white;
        [SerializeField] private Color progressDoneColor = new Color(1f, 0.82f, 0.25f, 1f);

        private static readonly Color Pending = new Color(0.2f, 0.22f, 0.34f, 1f);
        private static readonly Color Done = new Color(0.2f, 0.42f, 0.3f, 1f);

        public void Bind(string title, string description, string progress, bool done, Sprite iconSprite = null)
        {
            titleText.text = title;
            descriptionText.text = description;
            progressText.text = progress;
            progressText.color = done ? progressDoneColor : progressPendingColor;

            // A row drawn with a plaque sprite keeps the sprite's own colors; the old flat rows are tinted instead.
            if (background.sprite == null)
                background.color = done ? Done : Pending;

            if (icon != null)
            {
                icon.sprite = iconSprite;
                icon.enabled = iconSprite != null;
            }
            if (iconFrame != null && framePending != null)
                iconFrame.sprite = done && frameDone != null ? frameDone : framePending;
        }
    }
}
