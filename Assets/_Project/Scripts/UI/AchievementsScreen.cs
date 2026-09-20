using Rebonk.Core;
using Rebonk.Gameplay;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Scrollable list of all achievements: earned ones highlighted, the rest with progress.</summary>
    public class AchievementsScreen : MonoBehaviour
    {
        [SerializeField] private InfoRow template;
        [SerializeField] private RectTransform content;
        [SerializeField] private ScrollRect scroll;
        [SerializeField] private Text counterText;

        private readonly List<InfoRow> _rows = new List<InfoRow>();

        private void Awake() => template.gameObject.SetActive(false);

        private void OnEnable()
        {
            Loc.Changed += Refresh;
            Refresh();
            scroll.verticalNormalizedPosition = 1f;
        }

        private void OnDisable() => Loc.Changed -= Refresh;

        private void Refresh()
        {
            var data = SaveSystem.Data;
            var all = Achievements.All;
            while (_rows.Count < all.Count)
            {
                var row = Instantiate(template, content);
                row.gameObject.SetActive(true);
                _rows.Add(row);
            }

            for (var i = 0; i < all.Count; i++)
            {
                var a = all[i];
                var earned = Achievements.IsEarned(data, a);
                var progress = a.rule.ProgressText(data);
                _rows[i].Bind(Loc.T(a.name), a.rule.Describe(), earned ? Loc.T("Done") : progress, earned);
            }

            counterText.text = Loc.F("Earned {0} / {1}", Achievements.EarnedCount(data), all.Count);
        }
    }
}
