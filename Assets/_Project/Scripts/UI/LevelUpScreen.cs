using Rebonk.Core;
using System.Collections.Generic;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// Pauses the game and lets the player pick one of a few cards: after a level-up the cards are weapons /
    /// items, after a totem activation they are run-long stat blessings. Several requests queue up and are shown in turn.
    /// </summary>
    public class LevelUpScreen : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private UpgradeCard[] cards;
        [SerializeField] private Inventory inventory;
        [SerializeField] private BlessingPool blessingPool;
        [SerializeField] private Text titleText;
        [SerializeField, Range(1, 4)] private int choiceCount = 3;
        [SerializeField, Range(1, 4)] private int blessingChoices = 3;

        private static readonly Color LevelUpColor = new Color(1f, 0.9f, 0.3f, 1f);
        private static readonly Color TotemColor = new Color(0.45f, 0.9f, 1f, 1f);

        private readonly List<UpgradeChoice> _choices = new List<UpgradeChoice>();
        private PlayerLevel _level;
        private int _pendingLevelUps;
        private int _pendingTotems;
        private bool _totemMode;
        private bool _open;

        private void Start()
        {
            panel.SetActive(false);
            _level = PlayerController.Instance.Level;
            _level.LeveledUp += OnLeveledUp;
            Totem.Activated += OnTotemActivated;
        }

        private void OnDestroy()
        {
            if (_level != null)
                _level.LeveledUp -= OnLeveledUp;
            Totem.Activated -= OnTotemActivated;
            Time.timeScale = 1f;
        }

        private void OnLeveledUp(int newLevel)
        {
            _pendingLevelUps++;
            if (!_open)
                Open();
        }

        private void OnTotemActivated(Totem totem)
        {
            _pendingTotems++;
            if (!_open)
                Open();
        }

        private void Open()
        {
            // Totem blessings first: the player just spent 5 seconds earning them.
            _totemMode = _pendingTotems > 0;

            if (_totemMode)
            {
                blessingPool.Roll(Mathf.Min(blessingChoices, cards.Length), _choices);
            }
            else
            {
                var player = PlayerController.Instance;
                // Some characters and items (Lucky Lucy, Lucky Bookmark) are offered more cards.
                var count = Mathf.Max(choiceCount, player.Base.levelUpChoices) + player.Stats.ExtraChoices;
                inventory.GetChoices(Mathf.Clamp(count, 1, cards.Length), _choices);
            }

            if (_choices.Count == 0)
            {
                if (_totemMode) _pendingTotems = 0; else _pendingLevelUps = 0;
                if (_pendingLevelUps > 0 || _pendingTotems > 0)
                    Open();
                else
                    Close();
                return;
            }

            if (titleText != null)
            {
                titleText.text = Loc.T(_totemMode ? "TOTEM BLESSING!" : "LEVEL UP!");
                titleText.color = _totemMode ? TotemColor : LevelUpColor;
            }

            if (!_open)
                HudVisibility.Push(); // hide the HUD behind the window while it is open
            _open = true;
            Time.timeScale = 0f;
            panel.SetActive(true);

            for (var i = 0; i < cards.Length; i++)
            {
                var active = i < _choices.Count;
                cards[i].gameObject.SetActive(active);
                if (active)
                    cards[i].Bind(_choices[i], OnPicked);
            }
        }

        private void OnPicked(UpgradeChoice choice)
        {
            inventory.Apply(choice);
            if (_totemMode)
                _pendingTotems--;
            else
                _pendingLevelUps--;

            if (_pendingLevelUps > 0 || _pendingTotems > 0)
                Open();
            else
                Close();
        }

        private void Close()
        {
            if (_open)
                HudVisibility.Pop();
            _open = false;
            panel.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
