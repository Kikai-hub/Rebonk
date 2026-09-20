using Rebonk.Core;
using System;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One selectable upgrade card on the level-up screen.</summary>
    public class UpgradeCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private Text title;
        [SerializeField] private Text levelLabel;
        [SerializeField] private Text description;

        public void Bind(UpgradeChoice choice, Action<UpgradeChoice> onPicked)
        {
            var data = choice.Data;
            title.text = Loc.T(data.displayName);
            var blessing = data is BlessingData;
            levelLabel.text = blessing ? Loc.T("THIS RUN") : choice.IsEvolution ? Loc.T("EVOLUTION!") : choice.NextLevel == 1 ? Loc.T("NEW") : Loc.F("Lv {0}", choice.NextLevel);
            levelLabel.color = blessing ? new Color(0.45f, 0.9f, 1f, 1f) : choice.IsEvolution ? new Color(1f, 0.55f, 0.1f, 1f) : new Color(1f, 0.9f, 0.3f, 1f);
            description.text = Loc.T(data.GetCardText(choice.NextLevel));
            icon.sprite = data.icon;
            icon.enabled = data.icon != null;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onPicked(choice));
        }
    }
}
