using System.Collections.Generic;
using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// The weapons &amp; items collection screen: a filterable, scrollable grid of cards (rarity shown on
    /// each card) and a detail panel on the right for whichever card was last clicked. Nothing here is
    /// selected/equipped — weapons and items are only ever obtained by unlocking + drawing them on level-up.
    /// </summary>
    public class WeaponsScreen : MonoBehaviour
    {
        [SerializeField] private MetaCatalog meta;

        [Header("Grid")]
        [SerializeField] private WeaponEntry cardTemplate;
        [SerializeField] private RectTransform cardContent;
        [SerializeField] private ScrollRect cardScroll;
        [Tooltip("All, Near, Ranged, Radius, Passive (in this order).")]
        [SerializeField] private Button[] filterButtons;
        [SerializeField] private Text unlockedCounter;

        [Header("Detail panel")]
        [SerializeField] private Image detailIcon;
        [SerializeField] private Text detailName;
        [SerializeField] private Text detailRarity;
        [SerializeField] private Text detailDescription;
        [SerializeField] private Text statLabel1;
        [SerializeField] private Text statValue1;
        [SerializeField] private Text statLabel2;
        [SerializeField] private Text statValue2;
        [SerializeField] private Text statLabel3;
        [SerializeField] private Text statValue3;

        private static readonly Color FilterOn = new Color(0.55f, 1f, 0.7f, 1f);
        private static readonly Color FilterOff = Color.white;

        private readonly List<WeaponEntry> _cards = new List<WeaponEntry>();
        private readonly List<UpgradeData> _all = new List<UpgradeData>();
        private readonly List<UpgradeData> _filtered = new List<UpgradeData>();
        private int _filter; // 0 all, 1 near, 2 ranged, 3 radius, 4 passive
        private UpgradeData _selected;

        private void Awake()
        {
            cardTemplate.gameObject.SetActive(false);
        }

        private void Start()
        {
            for (var i = 0; i < filterButtons.Length; i++)
            {
                var index = i;
                filterButtons[i].onClick.AddListener(() => { _filter = index; Refresh(); ScrollToTop(); });
            }
        }

        private void OnEnable()
        {
            Loc.Changed += Refresh;
            Refresh();
            ScrollToTop();
        }

        private void ScrollToTop()
        {
            if (cardScroll != null)
                cardScroll.verticalNormalizedPosition = 1f;
        }

        private void OnDisable() => Loc.Changed -= Refresh;

        private void Refresh()
        {
            var save = SaveSystem.Data;

            _all.Clear();
            foreach (var w in meta.Weapons()) _all.Add(w);
            foreach (var p in meta.Passives()) _all.Add(p);

            _filtered.Clear();
            foreach (var data in _all)
            {
                if (!Matches(data)) continue;
                _filtered.Add(data);
            }

            if (_selected == null || !_filtered.Contains(_selected))
                _selected = _filtered.Count > 0 ? _filtered[0] : null;

            while (_cards.Count < _filtered.Count)
            {
                var card = Instantiate(cardTemplate, cardContent);
                card.gameObject.SetActive(true);
                _cards.Add(card);
            }

            var unlockedCount = 0;
            foreach (var data in _all)
                if (!(data is WeaponData w) || w.unlock.IsUnlocked)
                    unlockedCount++;

            for (var i = 0; i < _cards.Count; i++)
            {
                var has = i < _filtered.Count;
                _cards[i].gameObject.SetActive(has);
                if (!has) continue;

                var data = _filtered[i];
                _cards[i].Bind(data, save, data == _selected, () => { _selected = data; Refresh(); });
            }

            for (var i = 0; i < filterButtons.Length; i++)
                filterButtons[i].targetGraphic.color = i == _filter ? FilterOn : FilterOff;

            unlockedCounter.text = Loc.F("Unlocked {0} / {1}     (showing {2})", unlockedCount, _all.Count, _filtered.Count);

            ShowDetail(_selected, save);
        }

        private bool Matches(UpgradeData data)
        {
            switch (_filter)
            {
                case 1: return data is WeaponData w1 && w1.category == WeaponCategory.Near;
                case 2: return data is WeaponData w2 && w2.category == WeaponCategory.Ranged;
                case 3: return data is WeaponData w3 && w3.category == WeaponCategory.Radius;
                case 4: return data is PassiveData;
                default: return true;
            }
        }

        private void ShowDetail(UpgradeData data, SaveData save)
        {
            if (data == null)
            {
                detailIcon.enabled = false;
                detailName.text = detailRarity.text = detailDescription.text = "";
                statLabel1.text = statValue1.text = statLabel2.text = statValue2.text = statLabel3.text = statValue3.text = "";
                return;
            }

            var unlocked = !(data is WeaponData w) || w.unlock.IsUnlocked;

            detailIcon.sprite = data.icon;
            detailIcon.enabled = unlocked && data.icon != null;
            detailName.text = unlocked ? Loc.T(data.displayName) : Loc.T("???");
            detailRarity.text = unlocked ? Loc.T(RarityInfo.Key(data.rarity)) : "";
            detailRarity.color = RarityInfo.Color(data.rarity);

            if (!unlocked && data is WeaponData locked)
            {
                var progress = locked.unlock.ProgressText(save);
                detailDescription.text = locked.unlock.Describe() + (progress.Length > 0 ? " (" + progress + ")" : "");
                statLabel1.text = statValue1.text = statLabel2.text = statValue2.text = statLabel3.text = statValue3.text = "";
                return;
            }

            detailDescription.text = Loc.T(data.description);

            if (data is WeaponData weapon)
            {
                var lvl = weapon.GetLevel(1);
                statLabel1.text = Loc.T("DAMAGE");
                statValue1.text = lvl.damage.ToString("0.#");
                statLabel2.text = Loc.T("ATTACK SPEED");
                statValue2.text = (1f / Mathf.Max(0.05f, lvl.cooldown)).ToString("0.0");
                statLabel3.text = Loc.T("AREA");
                statValue3.text = lvl.area.ToString("0.0");
            }
            else if (data is PassiveData passive)
            {
                statLabel1.text = Loc.T("Max level");
                statValue1.text = passive.maxLevel.ToString();
                statLabel2.text = statValue2.text = statLabel3.text = statValue3.text = "";
            }
        }
    }
}
