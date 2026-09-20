using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// Pause window opened by the PAUSE button: shows the current run and offers RESUME, RESTART and MAIN MENU.
    /// Leaving the run needs a second tap so it cannot happen by accident.
    /// </summary>
    public class PauseScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Text restartLabel;
        [SerializeField] private Text menuLabel;
        [SerializeField] private Text statsText;
        [SerializeField] private string menuSceneName = "MainMenu";
        [SerializeField] private float confirmSeconds = 3f;

        private enum Pending { None, Restart, Menu }

        private Pending _pending;
        private float _pendingUntil;
        private bool _open;

        private void Start()
        {
            panel.SetActive(false);
            pauseButton.onClick.AddListener(Open);
            resumeButton.onClick.AddListener(Close);
            restartButton.onClick.AddListener(() => Confirm(Pending.Restart));
            menuButton.onClick.AddListener(() => Confirm(Pending.Menu));
        }

        private void OnDestroy()
        {
            if (_open)
                HudVisibility.Pop();
        }

        private void Update()
        {
            // The "TAP AGAIN" confirmation expires (real time: the game is paused).
            if (_pending != Pending.None && Time.unscaledTime >= _pendingUntil)
                ClearPending();
        }

        private void Open()
        {
            if (_open)
                return;

            _open = true;
            HudVisibility.Push();
            Time.timeScale = 0f;
            RefreshStats();
            ClearPending();
            panel.SetActive(true);
        }

        private void Close()
        {
            if (!_open)
                return;

            _open = false;
            panel.SetActive(false);
            HudVisibility.Pop();
            Time.timeScale = 1f;
            ClearPending();
        }

        private void RefreshStats()
        {
            var zone = ZoneTimer.Instance != null ? ZoneTimer.Instance.ZoneLevel : 1;
            var seconds = RunClock.Instance != null ? Mathf.FloorToInt(RunClock.Instance.Elapsed) : 0;
            var kills = RunTracker.Instance != null ? RunTracker.Instance.Kills : 0;
            var level = PlayerController.Instance != null ? PlayerController.Instance.Level.Level : 1;
            statsText.text = Loc.F("Zone {0}     Time {1}\nLevel {2}     Kills {3}", zone, (seconds / 60) + ":" + (seconds % 60).ToString("00"), level, kills);
        }

        private void Confirm(Pending action)
        {
            if (_pending != action)
            {
                _pending = action;
                _pendingUntil = Time.unscaledTime + confirmSeconds;
                restartLabel.text = action == Pending.Restart ? Loc.T("TAP AGAIN TO RESTART") : Loc.T("RESTART RUN");
                menuLabel.text = action == Pending.Menu ? Loc.T("TAP AGAIN TO LEAVE") : Loc.T("MAIN MENU");
                return;
            }

            if (RunTracker.Instance != null)
                RunTracker.Instance.FinishRun(); // leaving counts as ending the run

            Time.timeScale = 1f;
            if (action == Pending.Restart)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            else
                SceneManager.LoadScene(menuSceneName);
        }

        private void ClearPending()
        {
            _pending = Pending.None;
            restartLabel.text = Loc.T("RESTART RUN");
            menuLabel.text = Loc.T("MAIN MENU");
        }
    }
}
