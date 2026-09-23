using Rebonk.Core;
using Rebonk.Gameplay;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One card in the weapons &amp; items collection grid. Clicking one only previews it in
    /// <see cref="WeaponsScreen"/>'s detail panel; nothing is ever selected/equipped from this screen.</summary>
    public class WeaponEntry : MonoBehaviour
    {
        [SerializeField] private Image background;
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Text nameText;
        [SerializeField] private Text rarityText;
        [SerializeField] private Text statLine;
        [SerializeField] private Image[] rarityDots;

        private static readonly Color NormalBg = Color.white; // shows button_navy.png's own painted color, like every other plaque in the UI
        private static readonly Color PreviewedBg = new Color(1f, 0.82f, 0.5f, 1f);
        private static readonly Color LockedBg = new Color(0.3f, 0.3f, 0.35f, 1f);
        private static readonly Color DotOff = new Color(0.3f, 0.32f, 0.4f, 1f);

        public void Bind(UpgradeData data, SaveData save, bool previewed, Action onClick)
        {
            var unlocked = !(data is WeaponData w) || w.unlock.IsUnlocked;

            background.color = !unlocked ? LockedBg : previewed ? PreviewedBg : NormalBg;
            icon.sprite = data.icon;
            icon.enabled = unlocked && data.icon != null;
            if (lockIcon != null) lockIcon.SetActive(!unlocked);

            nameText.text = unlocked ? Loc.T(data.displayName) : Loc.T("???");
            rarityText.text = unlocked ? Loc.T(RarityInfo.Key(data.rarity)) : "";
            rarityText.color = RarityInfo.Color(data.rarity);

            if (!unlocked && data is WeaponData weapon)
            {
                var progress = weapon.unlock.ProgressText(save);
                statLine.text = weapon.unlock.Describe() + (progress.Length > 0 ? " (" + progress + ")" : "");
                statLine.color = new Color(1f, 0.8f, 0.4f, 1f);
            }
            else if (data is WeaponData w2)
            {
                var lvl = w2.GetLevel(1);
                statLine.text = Loc.T("DAMAGE") + " " + lvl.damage.ToString("0.#") + "   " + Loc.T("ATTACK SPEED") + " " + (1f / Mathf.Max(0.05f, lvl.cooldown)).ToString("0.0");
                statLine.color = new Color(0.75f, 0.8f, 0.9f, 1f);
            }
            else
            {
                statLine.text = Loc.T(data.description);
                statLine.color = new Color(0.75f, 0.8f, 0.9f, 1f);
            }

            var filled = unlocked ? (int)data.rarity + 1 : 0;
            for (var i = 0; i < rarityDots.Length; i++)
                rarityDots[i].color = i < filled ? RarityInfo.Color(data.rarity) : DotOff;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }
    }
}
