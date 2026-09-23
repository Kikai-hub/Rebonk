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
        [Header("Selected language look (empty sprites = old flat color tint)")]
        [SerializeField] private Sprite languageOnSprite;
        [SerializeField] private Sprite languageOffSprite;
        [SerializeField] private Color languageOnText = new Color(1f, 0.85f, 0.3f, 1f);
        [SerializeField] private Color languageOffText = Color.white;

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
            {
                var on = i == (int)Loc.Current;
                var button = languageButtons[i];
                if (languageOnSprite != null && button.targetGraphic is Image image)
                {
                    image.sprite = on ? languageOnSprite : languageOffSprite;
                    image.color = Color.white;
                    button.GetComponentInChildren<Text>().color = on ? languageOnText : languageOffText;
                }
                else
                {
                    button.targetGraphic.color = on ? On : Off;
                }

                // Optional "Selected" child (e.g. gold diamonds on both sides) shown only on the active language.
                var marker = button.transform.Find("Selected");
                if (marker != null)
                    marker.gameObject.SetActive(on);
            }
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
