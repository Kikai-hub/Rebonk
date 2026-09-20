using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>A list row for quests and achievements: title, description, progress, and a done tint.</summary>
    public class InfoRow : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text progressText;

        private static readonly Color Pending = new Color(0.2f, 0.22f, 0.34f, 1f);
        private static readonly Color Done = new Color(0.2f, 0.42f, 0.3f, 1f);

        public void Bind(string title, string description, string progress, bool done)
        {
            titleText.text = title;
            descriptionText.text = description;
            progressText.text = progress;
            progressText.color = done ? new Color(0.6f, 1f, 0.7f, 1f) : Color.white;
            background.color = done ? Done : Pending;
        }
    }
}
