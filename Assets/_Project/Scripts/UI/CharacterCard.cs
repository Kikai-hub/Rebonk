using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One small square thumbnail in the character carousel: portrait, locked/selected state, nothing else.
    /// Clicking one only <i>previews</i> that character (see <see cref="CharactersScreen"/>); it does not select it.</summary>
    public class CharacterCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image portrait;
        [SerializeField] private Image background;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject equippedMark;

        private static readonly Color Normal = new Color(0.14f, 0.16f, 0.24f, 1f);
        private static readonly Color Previewed = new Color(1f, 0.82f, 0.3f, 1f);
        private static readonly Color Locked = new Color(0.06f, 0.06f, 0.09f, 1f);

        public void Bind(Gameplay.CharacterStats character, bool unlocked, bool previewed, bool equipped, Action onClick)
        {
            portrait.sprite = character.sprite;
            portrait.enabled = unlocked;
            if (lockIcon != null) lockIcon.SetActive(!unlocked);
            if (equippedMark != null) equippedMark.SetActive(unlocked && equipped);

            background.color = !unlocked ? Locked : previewed ? Previewed : Normal;

            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            if (unlocked)
                button.onClick.AddListener(() => onClick());
        }
    }
}
