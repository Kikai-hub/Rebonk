using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// Shown when the player dies: dim overlay, GAME OVER, run statistics, newly unlocked things,
    /// and buttons to play again or go back to the menu. (Full results screen comes with the UI stage.)
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text statsText;
        [SerializeField] private Text unlocksText;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private string menuSceneName = "MainMenu";

        private void Start()
        {
            panel.SetActive(false);
            PlayerController.Instance.Health.Died += OnPlayerDied;
            playAgainButton.onClick.AddListener(PlayAgain);
            menuButton.onClick.AddListener(ToMenu);
        }

        private void OnDestroy()
        {
            var player = PlayerController.Instance;
            if (player != null)
                player.Health.Died -= OnPlayerDied;
        }

        private void OnPlayerDied(Rebonk.Core.Health _)
        {
            var before = SaveSystem.Data;
            var bestKills = before.bestKills;
            var bestSurvival = before.bestSurvival;
            var bestLevel = before.bestLevel;

            var result = RunTracker.Instance != null ? RunTracker.Instance.FinishRun() : default;
            var text = Loc.F("Kills: {0}     Zones cleared: {1}\nSurvived vs spirits: {2}", result.Kills, result.ZonesCleared, Clock(result.SurvivalSeconds))
                       + "\n" + Loc.F("Level {0}     Run time {1}", result.Level, Clock(result.RunSeconds));

            var records = new System.Collections.Generic.List<string>();
            if (result.Kills > bestKills) records.Add(Loc.T("kills"));
            if (result.SurvivalSeconds > bestSurvival + 0.5f) records.Add(Loc.T("survival time"));
            if (result.Level > bestLevel) records.Add(Loc.T("level"));
            if (records.Count > 0)
                text += "\n" + Loc.F("NEW RECORD: {0}", string.Join(", ", records));
            statsText.text = text;

            var lines = new System.Collections.Generic.List<string>();
            if (result.QuestsCompleted != null && result.QuestsCompleted.Count > 0)
                lines.Add(Loc.F("QUEST DONE: {0}", string.Join(", ", result.QuestsCompleted)));
            if (result.NewAchievements != null && result.NewAchievements.Count > 0)
                lines.Add(Loc.F("ACHIEVEMENT: {0}", string.Join(", ", LocalizedNames(result.NewAchievements))));
            var unlocks = result.NewUnlocks;
            if (unlocks != null && unlocks.Count > 0)
                lines.Add(Loc.F("NEW UNLOCK: {0}!", string.Join(", ", LocalizedNames(unlocks))));
            unlocksText.text = string.Join("\n", lines);
            HudVisibility.Push(); // never popped: the run is over
            panel.SetActive(true);
        }

        private static System.Collections.Generic.List<string> LocalizedNames(System.Collections.Generic.List<string> names)
        {
            var list = new System.Collections.Generic.List<string>(names.Count);
            foreach (var n in names)
                list.Add(Loc.T(n));
            return list;
        }

        private static string Clock(float seconds)
        {
            var s = Mathf.FloorToInt(seconds);
            return (s / 60) + ":" + (s % 60).ToString("00");
        }

        private static void PlayAgain()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
