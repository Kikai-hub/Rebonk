using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One weapon in the collection screen.</summary>
    public class WeaponEntry : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Text nameText;
        [SerializeField] private Text statusText;

        public void Bind(WeaponData weapon, SaveData save)
        {
            var unlocked = weapon.unlock.IsUnlocked;
            icon.sprite = weapon.icon;
            icon.color = unlocked ? Color.white : new Color(0.15f, 0.15f, 0.15f, 1f);
            nameText.text = unlocked ? Loc.T(weapon.displayName) : "???";

            if (unlocked)
            {
                statusText.text = Loc.T("Unlocked");
                statusText.color = new Color(0.5f, 1f, 0.6f, 1f);
            }
            else
            {
                var progress = weapon.unlock.ProgressText(save);
                statusText.text = weapon.unlock.Describe() + (progress.Length > 0 ? " (" + progress + ")" : "");
                statusText.color = new Color(1f, 0.8f, 0.4f, 1f);
            }
        }
    }
}
