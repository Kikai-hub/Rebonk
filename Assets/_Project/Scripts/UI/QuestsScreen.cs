using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Today's three daily quests with progress and the time left until they rotate.</summary>
    public class QuestsScreen : MonoBehaviour
    {
        [SerializeField] private InfoRow[] rows;
        [SerializeField] private Text resetText;
        [SerializeField] private Text totalText;

        private float _nextTick;

        private void OnEnable()
        {
            Loc.Changed += Refresh;
            Refresh();
        }

        private void OnDisable() => Loc.Changed -= Refresh;

        private void Update()
        {
            if (Time.unscaledTime < _nextTick)
                return;
            _nextTick = Time.unscaledTime + 1f;

            // Past midnight while the screen is open: new quests.
            if (SaveSystem.Data.dailyDate != DailyQuests.Today)
                Refresh();
            else
                UpdateResetText();
        }

        private void Refresh()
        {
            var data = SaveSystem.Data;
            DailyQuests.EnsureToday(data);
            for (var i = 0; i < rows.Length; i++)
            {
                var quest = DailyQuests.Get(data, i);
                var done = data.dailyDone[i];
                rows[i].Bind(Loc.F("Quest {0}", i + 1), quest.Describe(), done ? Loc.T("Done") : quest.ProgressText(data.dailyProgress[i]), done);
            }
            totalText.text = Loc.F("Quests completed in total: {0}", data.questsCompleted);
            UpdateResetText();
        }

        private void UpdateResetText()
        {
            var left = DailyQuests.TimeUntilReset;
            resetText.text = Loc.F("New quests in {0}", ((int)left.TotalHours) + ":" + left.Minutes.ToString("00") + ":" + left.Seconds.ToString("00"));
        }
    }
}
