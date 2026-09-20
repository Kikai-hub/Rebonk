using Rebonk.Core;
using System.Collections.Generic;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// The hub: main panel plus world select, a swipeable character carousel and a scrollable,
    /// filterable weapon collection. Cards are cloned from templates so any number of items fits.
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

        [Header("Characters carousel")]
        [SerializeField] private CharacterCard characterTemplate;
        [SerializeField] private RectTransform characterContent;
        [SerializeField] private ScrollRect characterScroll;
        [SerializeField] private Button previousPageButton;
        [SerializeField] private Button nextPageButton;
        [SerializeField] private Text characterCounter;

        [Header("Weapon collection")]
        [SerializeField] private WeaponEntry weaponTemplate;
        [SerializeField] private RectTransform weaponContent;
        [SerializeField] private ScrollRect weaponScroll;
        [Tooltip("All, Near, Ranged, Radius (in this order).")]
        [SerializeField] private Button[] filterButtons;
        [SerializeField] private Text weaponCounter;

        private readonly List<CharacterCard> _characterCards = new List<CharacterCard>();
        private readonly List<WeaponEntry> _weaponEntries = new List<WeaponEntry>();
        private List<WeaponData> _weapons;
        private int _filter; // 0 = all, 1..3 = category + 1

        private static readonly Color FilterOn = new Color(0.3f, 0.55f, 0.4f, 1f);
        private static readonly Color FilterOff = new Color(0.25f, 0.28f, 0.42f, 1f);

        private void Start()
        {
            characterTemplate.gameObject.SetActive(false);
            weaponTemplate.gameObject.SetActive(false);

            playButton.onClick.AddListener(() => Show(worldPanel));
            charactersButton.onClick.AddListener(() => { RefreshCharacters(); Show(charactersPanel); });
            weaponsButton.onClick.AddListener(() => { RefreshWeapons(); Show(weaponsPanel); });
            foreach (var b in backButtons)
                b.onClick.AddListener(() => Show(mainPanel));

            previousPageButton.onClick.AddListener(() => ScrollCharacters(-1));
            nextPageButton.onClick.AddListener(() => ScrollCharacters(1));
            for (var i = 0; i < filterButtons.Length; i++)
            {
                var index = i;
                filterButtons[i].onClick.AddListener(() => { _filter = index; RefreshWeapons(); });
            }

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
            if (charactersPanel.activeSelf)
                RefreshCharacters();
            if (weaponsPanel.activeSelf)
                RefreshWeapons();
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

        // ---------- characters ----------

        private void RefreshCharacters()
        {
            var save = SaveSystem.Data;
            var selected = meta.SelectedCharacter();

            while (_characterCards.Count < meta.characters.Count)
            {
                var card = Instantiate(characterTemplate, characterContent);
                card.gameObject.SetActive(true);
                _characterCards.Add(card);
            }

            var unlocked = 0;
            for (var i = 0; i < _characterCards.Count; i++)
            {
                var has = i < meta.characters.Count;
                _characterCards[i].gameObject.SetActive(has);
                if (!has)
                    continue;

                var character = meta.characters[i];
                if (character.unlock.IsUnlocked)
                    unlocked++;
                _characterCards[i].Bind(character, save, character == selected, OnCharacterSelected);
            }

            characterCounter.text = Loc.F("Unlocked {0} / {1}     (swipe or use the arrows)", unlocked, meta.characters.Count);
        }

        private void OnCharacterSelected(CharacterStats character)
        {
            SaveSystem.Data.selectedCharacterId = character.unlock.id;
            SaveSystem.Save();
            RefreshCharacters();
        }

        /// <summary>Scrolls the carousel by about three cards.</summary>
        private void ScrollCharacters(int direction)
        {
            var viewport = characterScroll.viewport != null ? characterScroll.viewport : (RectTransform)characterScroll.transform;
            var scrollable = characterContent.rect.width - viewport.rect.width;
            if (scrollable <= 1f)
                return;

            var step = viewport.rect.width * 0.75f / scrollable;
            characterScroll.horizontalNormalizedPosition = Mathf.Clamp01(characterScroll.horizontalNormalizedPosition + direction * step);
        }

        // ---------- weapons ----------

        private void RefreshWeapons()
        {
            var save = SaveSystem.Data;
            if (_weapons == null)
                _weapons = new List<WeaponData>(meta.Weapons());

            while (_weaponEntries.Count < _weapons.Count)
            {
                var entry = Instantiate(weaponTemplate, weaponContent);
                entry.gameObject.SetActive(true);
                _weaponEntries.Add(entry);
            }

            int unlocked = 0, shown = 0;
            for (var i = 0; i < _weapons.Count; i++)
            {
                var weapon = _weapons[i];
                if (weapon.unlock.IsUnlocked)
                    unlocked++;

                var visible = _filter == 0 || (int)weapon.category == _filter - 1;
                _weaponEntries[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;

                shown++;
                _weaponEntries[i].Bind(weapon, save);
            }

            for (var i = 0; i < filterButtons.Length; i++)
                filterButtons[i].targetGraphic.color = i == _filter ? FilterOn : FilterOff;

            weaponCounter.text = Loc.F("Unlocked {0} / {1}     (showing {2})", unlocked, _weapons.Count, shown);
            weaponScroll.verticalNormalizedPosition = 1f;
        }
    }
}
