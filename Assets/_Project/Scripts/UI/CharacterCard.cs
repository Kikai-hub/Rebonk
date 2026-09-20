using Rebonk.Core;
using System;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One character on the characters screen: locked (with unlock condition and progress), selectable or selected.</summary>
    public class CharacterCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image portrait;
        [SerializeField] private Image background;
        [SerializeField] private Text nameText;
        [SerializeField] private Text statsText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Text statusText;

        private static readonly Color Normal = new Color(0.16f, 0.18f, 0.28f, 1f);
        private static readonly Color SelectedColor = new Color(0.2f, 0.42f, 0.3f, 1f);
        private static readonly Color Locked = new Color(0.1f, 0.11f, 0.16f, 1f);

        private static string StartingWeapons(CharacterStats c)
        {
            if (c.randomStartWeapon)
                return Loc.T("a random weapon");

            var text = c.startingWeapon != null ? Loc.T(c.startingWeapon.displayName) : "-";
            foreach (var extra in c.extraStartingWeapons)
                if (extra != null)
                    text += " + " + Loc.T(extra.displayName);
            return text;
        }

        public void Bind(CharacterStats character, SaveData save, bool selected, Action<CharacterStats> onSelect)
        {
            var unlocked = character.unlock.IsUnlocked;

            nameText.text = unlocked ? Loc.T(character.displayName) : "???";
            portrait.sprite = character.sprite;
            portrait.color = unlocked ? Color.white : new Color(0f, 0f, 0f, 0.85f); // silhouette while locked
            background.color = !unlocked ? Locked : selected ? SelectedColor : Normal;

            statsText.text = unlocked
                ? Loc.F("HP {0}   SPD {1}   DMG x{2}\nStarts with: {3}", character.maxHp, character.moveSpeed.ToString("0.0"), character.damageMultiplier.ToString("0.0#"), StartingWeapons(character))
                : "";
            descriptionText.text = unlocked
                ? (string.IsNullOrEmpty(character.traitName) ? "" : "<b>" + Loc.T(character.traitName) + "</b>\n") + Loc.T(character.description)
                : "";

            if (unlocked)
            {
                statusText.text = selected ? Loc.T("SELECTED") : Loc.T("SELECT");
                statusText.color = selected ? new Color(0.5f, 1f, 0.6f, 1f) : Color.white;
            }
            else
            {
                var progress = character.unlock.ProgressText(save);
                statusText.text = Loc.T("LOCKED") + "\n" + character.unlock.Describe() + (progress.Length > 0 ? "\n(" + progress + ")" : "");
                statusText.color = new Color(1f, 0.8f, 0.4f, 1f);
            }

            button.interactable = unlocked;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onSelect(character));
        }
    }
}
