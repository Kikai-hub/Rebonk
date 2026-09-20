using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Settings window: language, music and effects volume, and a two-tap "reset progress".</summary>
    public class SettingsScreen : MonoBehaviour
    {
        [Tooltip("In the order of the Language enum.")]
        [SerializeField] private Button[] languageButtons;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text resetLabel;
        [SerializeField] private float confirmSeconds = 3f;

        private static readonly Color On = new Color(0.3f, 0.55f, 0.4f, 1f);
        private static readonly Color Off = new Color(0.3f, 0.3f, 0.45f, 1f);

        private float _confirmUntil;

        private void Awake()
        {
            for (var i = 0; i < languageButtons.Length; i++)
            {
                var language = (Language)i;
                languageButtons[i].GetComponentInChildren<Text>().text = Loc.NativeName(language);
                languageButtons[i].onClick.AddListener(() => Loc.Set(language));
            }

            musicSlider.onValueChanged.AddListener(v => GameSettings.MusicVolume = v);
            sfxSlider.onValueChanged.AddListener(v => { GameSettings.SfxVolume = v; Sound.Play(SfxId.UiClick); });
            resetButton.onClick.AddListener(OnResetClicked);
        }

        private void OnEnable()
        {
            Loc.Changed += Refresh;
            musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
            sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
            _confirmUntil = 0f;
            Refresh();
        }

        private void OnDisable()
        {
            Loc.Changed -= Refresh;
            PlayerPrefs.Save();
        }

        private void Update()
        {
            if (_confirmUntil > 0f && Time.unscaledTime >= _confirmUntil)
            {
                _confirmUntil = 0f;
                Refresh();
            }
        }

        private void Refresh()
        {
            for (var i = 0; i < languageButtons.Length; i++)
                languageButtons[i].targetGraphic.color = i == (int)Loc.Current ? On : Off;
            resetLabel.text = Loc.T(_confirmUntil > 0f ? "TAP AGAIN TO ERASE" : "RESET PROGRESS");
        }

        private void OnResetClicked()
        {
            if (_confirmUntil <= 0f)
            {
                _confirmUntil = Time.unscaledTime + confirmSeconds;
                Refresh();
                return;
            }

            SaveSystem.DeleteSave();
            _confirmUntil = 0f;
            resetLabel.text = Loc.T("PROGRESS ERASED");
        }
    }
}
