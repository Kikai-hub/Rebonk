using System.Collections.Generic;
using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// The character-select screen: a small thumbnail carousel up top, a big diorama with a left stats
    /// panel and a right starting-weapon panel for whichever character is currently <i>previewed</i>, and a
    /// SELECT button that commits the preview as the equipped character. Previewing never saves by itself.
    /// </summary>
    public class CharactersScreen : MonoBehaviour
    {
        [SerializeField] private MetaCatalog meta;

        [Header("Carousel (small thumbnails)")]
        [SerializeField] private CharacterCard cardTemplate;
        [SerializeField] private RectTransform cardContent;
        [SerializeField] private ScrollRect cardScroll;
        [SerializeField] private Button scrollPrevButton;
        [SerializeField] private Button scrollNextButton;
        [SerializeField] private Text unlockedCounter;

        [Header("Diorama")]
        [SerializeField] private Image dioramaPortrait;
        [SerializeField] private Button pagePrevButton;
        [SerializeField] private Button pageNextButton;
        [SerializeField] private Text pageCounter;

        [Header("Left panel: the character")]
        [SerializeField] private Text charNameText;
        [SerializeField] private Text charDescriptionText;
        [SerializeField] private Text healthValue;
        [SerializeField] private Text speedValue;
        [SerializeField] private Text damageValue;
        [SerializeField] private Text traitNameText;
        [SerializeField] private Text traitDescriptionText;

        [Header("Right panel: the starting weapon")]
        [SerializeField] private Image weaponIcon;
        [SerializeField] private Text weaponNameText;
        [SerializeField] private Text weaponDescriptionText;
        [SerializeField] private Text weaponDamageValue;
        [SerializeField] private Text weaponSpeedValue;
        [SerializeField] private Text weaponAreaValue;
        [SerializeField] private Button selectButton;
        [SerializeField] private Text selectLabel;

        private readonly List<CharacterCard> _cards = new List<CharacterCard>();
        private int _previewIndex;

        private void Awake()
        {
            cardTemplate.gameObject.SetActive(false);
        }

        private void Start()
        {
            scrollPrevButton.onClick.AddListener(() => ScrollCards(-1));
            scrollNextButton.onClick.AddListener(() => ScrollCards(1));
            pagePrevButton.onClick.AddListener(() => Page(-1));
            pageNextButton.onClick.AddListener(() => Page(1));
            selectButton.onClick.AddListener(OnSelectClicked);
        }

        private void OnEnable()
        {
            Loc.Changed += Refresh;
            var selected = meta.SelectedCharacter();
            _previewIndex = Mathf.Max(0, meta.characters.IndexOf(selected));
            Refresh();
        }

        private void OnDisable() => Loc.Changed -= Refresh;

        private void Refresh()
        {
            var save = SaveSystem.Data;
            var equipped = meta.SelectedCharacter();

            while (_cards.Count < meta.characters.Count)
            {
                var card = Instantiate(cardTemplate, cardContent);
                card.gameObject.SetActive(true);
                _cards.Add(card);
            }

            var unlockedCount = 0;
            for (var i = 0; i < _cards.Count; i++)
            {
                var has = i < meta.characters.Count;
                _cards[i].gameObject.SetActive(has);
                if (!has)
                    continue;

                var character = meta.characters[i];
                var unlocked = character.unlock.IsUnlocked;
                if (unlocked)
                    unlockedCount++;
                var index = i;
                _cards[i].Bind(character, unlocked, i == _previewIndex, character == equipped, () => Preview(index));
            }

            unlockedCounter.text = Loc.F("Unlocked {0} / {1}", unlockedCount, meta.characters.Count);
            ShowDetail(meta.characters[_previewIndex], save, equipped);
        }

        private void Preview(int index)
        {
            _previewIndex = index;
            Refresh();
        }

        private void Page(int direction)
        {
            var count = meta.characters.Count;
            _previewIndex = (_previewIndex + direction + count) % count;
            // Skip locked characters when paging so PREV/NEXT never lands on an un-clickable slot.
            var guard = 0;
            while (!meta.characters[_previewIndex].unlock.IsUnlocked && guard++ < count)
                _previewIndex = (_previewIndex + direction + count) % count;
            Refresh();
        }

        private void ShowDetail(CharacterStats character, SaveData save, CharacterStats equipped)
        {
            var unlocked = character.unlock.IsUnlocked;
            pageCounter.text = (_previewIndex + 1) + " / " + meta.characters.Count;

            dioramaPortrait.sprite = character.sprite;
            dioramaPortrait.enabled = unlocked;

            charNameText.text = unlocked ? Loc.T(character.displayName) : Loc.T("???");
            charDescriptionText.text = unlocked ? Loc.T(character.description) : character.unlock.Describe();
            healthValue.text = unlocked ? character.maxHp.ToString("0") : "-";
            speedValue.text = unlocked ? character.moveSpeed.ToString("0.0") : "-";
            damageValue.text = unlocked ? "x" + character.damageMultiplier.ToString("0.0#") : "-";

            var hasTrait = unlocked && !string.IsNullOrEmpty(character.traitName);
            traitNameText.text = hasTrait ? Loc.T(character.traitName) : "";
            if (traitDescriptionText != null)
                traitDescriptionText.text = ""; // no separate trait blurb in the data yet; the bio above already covers it

            var weapon = character.startingWeapon;
            var weaponKnown = unlocked && !character.randomStartWeapon && weapon != null;
            if (weaponKnown)
            {
                weaponIcon.sprite = weapon.icon;
                weaponIcon.enabled = weapon.icon != null;
                weaponNameText.text = Loc.T(weapon.displayName);
                weaponDescriptionText.text = Loc.T(weapon.description);
                var level1 = weapon.GetLevel(1);
                weaponDamageValue.text = level1.damage.ToString("0.#");
                weaponSpeedValue.text = (1f / Mathf.Max(0.05f, level1.cooldown)).ToString("0.0");
                weaponAreaValue.text = level1.area.ToString("0.0");
            }
            else
            {
                weaponIcon.enabled = false;
                weaponNameText.text = unlocked && character.randomStartWeapon ? Loc.T("Random weapon") : "";
                weaponDescriptionText.text = "";
                weaponDamageValue.text = weaponSpeedValue.text = weaponAreaValue.text = "-";
            }

            var isEquipped = unlocked && character == equipped;
            selectButton.gameObject.SetActive(unlocked);
            selectButton.interactable = !isEquipped;
            selectLabel.text = isEquipped ? Loc.T("EQUIPPED") : Loc.T("SELECT");
        }

        private void OnSelectClicked()
        {
            var character = meta.characters[_previewIndex];
            if (!character.unlock.IsUnlocked)
                return;
            SaveSystem.Data.selectedCharacterId = character.unlock.id;
            SaveSystem.Save();
            Refresh();
        }

        /// <summary>Scrolls the thumbnail strip by about three cards (manual browsing, independent of the preview).</summary>
        private void ScrollCards(int direction)
        {
            var viewport = cardScroll.viewport != null ? cardScroll.viewport : (RectTransform)cardScroll.transform;
            var scrollable = cardContent.rect.width - viewport.rect.width;
            if (scrollable <= 1f)
                return;

            var step = viewport.rect.width * 0.75f / scrollable;
            cardScroll.horizontalNormalizedPosition = Mathf.Clamp01(cardScroll.horizontalNormalizedPosition + direction * step);
        }
    }
}
