using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Zone timer / countdown readout and the big phase banners.</summary>
    public class ZoneHud : MonoBehaviour
    {
        [SerializeField] private Text timerText;
        [SerializeField] private Text bannerText;
        [SerializeField] private float bannerSeconds = 3f;

        private static readonly Color ZoneColor = Color.white;
        private static readonly Color CountdownColor = new Color(1f, 0.3f, 0.3f, 1f);
        private static readonly Color ClearedColor = new Color(0.4f, 1f, 0.5f, 1f);

        private ZoneTimer _zone;
        private float _bannerLeft;

        private void Start()
        {
            _zone = ZoneTimer.Instance;
            _zone.PhaseChanged += OnPhaseChanged;
            BossController.Spawned += OnBossSpawned;
            BossController.Defeated += OnBossDefeated;
            OnPhaseChanged(_zone.Phase);
        }

        private void OnDestroy()
        {
            if (_zone != null)
                _zone.PhaseChanged -= OnPhaseChanged;
            BossController.Spawned -= OnBossSpawned;
            BossController.Defeated -= OnBossDefeated;
        }

        private void OnBossSpawned(BossController boss) => ShowBanner(Loc.F("{0} AWAKENS!", boss.DisplayName.ToUpper()), CountdownColor);

        private void OnBossDefeated(BossController boss) => ShowBanner(Loc.T("BOSS DEFEATED! ENTER THE PORTAL"), ClearedColor);

        private void OnPhaseChanged(ZonePhase phase)
        {
            switch (phase)
            {
                case ZonePhase.Zone: ShowBanner(Loc.F("ZONE {0}", _zone.ZoneLevel), ZoneColor); break;
                case ZonePhase.Survival: ShowBanner(Loc.T("THE SPIRITS ARE COMING!"), CountdownColor); break;
                case ZonePhase.Cleared: ShowBanner(Loc.T("ZONE CLEARED!"), ClearedColor); break;
            }
        }

        private static string FormatClock(int totalSeconds) =>
            (totalSeconds / 60) + ":" + (totalSeconds % 60).ToString("00");

        private void ShowBanner(string text, Color color)
        {
            bannerText.text = text;
            bannerText.color = color;
            _bannerLeft = bannerSeconds;
        }

        private void Update()
        {
            switch (_zone.Phase)
            {
                case ZonePhase.Zone:
                    timerText.text = Loc.F("ZONE {0}", _zone.ZoneLevel) + "   " + FormatClock(Mathf.CeilToInt(Mathf.Max(0f, _zone.TimeLeft)));
                    timerText.color = ZoneColor;
                    break;
                case ZonePhase.Survival:
                    // Counts UP: survival time is the score of this phase.
                    timerText.text = Loc.T("SURVIVED") + "   " + FormatClock(Mathf.FloorToInt(_zone.SurvivedSeconds));
                    timerText.color = CountdownColor;
                    break;
                default:
                    timerText.text = Loc.T("ZONE CLEARED");
                    timerText.color = ClearedColor;
                    break;
            }

            if (_bannerLeft > 0f)
            {
                _bannerLeft -= Time.deltaTime;
                var c = bannerText.color;
                c.a = Mathf.Clamp01(_bannerLeft / 0.7f); // fade out during the last 0.7 s
                bannerText.color = c;
            }
            else if (bannerText.text.Length > 0)
            {
                bannerText.text = "";
            }
        }
    }
}
