using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// The hub: main panel plus world select, the character-select screen (own <see cref="CharactersScreen"/>
    /// component) and the weapons &amp; items collection (own <see cref="WeaponsScreen"/> component).
    /// </summary>
    public class HubMenu : MonoBehaviour
    {
        [SerializeField] private MetaCatalog meta;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject worldPanel;
        [SerializeField] private GameObject charactersPanel;
        [SerializeField] private GameObject weaponsPanel;
        [SerializeField] private Text selectedLabel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject questsPanel;
        [SerializeField] private Button questsButton;
        [SerializeField] private GameObject achievementsPanel;
        [SerializeField] private Button achievementsButton;

        [Header("Main panel buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button charactersButton;
        [SerializeField] private Button weaponsButton;
        [SerializeField] private Button[] backButtons;

        private void Start()
        {
            playButton.onClick.AddListener(() => Show(worldPanel));
            charactersButton.onClick.AddListener(() => Show(charactersPanel)); // CharactersScreen refreshes itself on enable
            weaponsButton.onClick.AddListener(() => Show(weaponsPanel)); // WeaponsScreen refreshes itself on enable
            foreach (var b in backButtons)
                b.onClick.AddListener(() => Show(mainPanel));

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => Show(settingsPanel));
            if (questsButton != null)
                questsButton.onClick.AddListener(() => Show(questsPanel));
            if (achievementsButton != null)
                achievementsButton.onClick.AddListener(() => Show(achievementsPanel));
            Loc.Changed += OnLanguageChanged;
            OnLanguageChanged();

            Show(mainPanel);
        }

        private void OnDestroy() => Loc.Changed -= OnLanguageChanged;

        private void OnLanguageChanged()
        {
            if (mainPanel.activeSelf)
                selectedLabel.text = Loc.F("Playing as: {0}", Loc.T(meta.SelectedCharacter().displayName));
        }

        private void Show(GameObject panel)
        {
            mainPanel.SetActive(panel == mainPanel);
            worldPanel.SetActive(panel == worldPanel);
            charactersPanel.SetActive(panel == charactersPanel);
            weaponsPanel.SetActive(panel == weaponsPanel);
            if (settingsPanel != null)
                settingsPanel.SetActive(panel == settingsPanel);
            if (questsPanel != null)
                questsPanel.SetActive(panel == questsPanel);
            if (achievementsPanel != null)
                achievementsPanel.SetActive(panel == achievementsPanel);

            if (panel == mainPanel)
                selectedLabel.text = Loc.F("Playing as: {0}", Loc.T(meta.SelectedCharacter().displayName));
        }
    }
}
