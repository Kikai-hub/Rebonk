using Rebonk.Core;
using System;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One selectable world on the world-select screen. Clicking it starts a run there directly.</summary>
    public class WorldCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image preview;
        [SerializeField] private Text title;
        [SerializeField] private Text description;
        [SerializeField] private Text enemiesValue;
        [SerializeField] private Text lootValue;
        [SerializeField] private Text difficultyValue;

        private static readonly string[] EnemiesKeys = { "Mild", "Moderate", "Fierce" };
        private static readonly string[] LootKeys = { "Standard", "Good", "Scarce" };
        private static readonly string[] DifficultyKeys = { "Easy", "Balanced", "Brutal" };
        private static readonly Color[] TierColors =
        {
            new Color(0.55f, 1f, 0.7f, 1f),
            new Color(1f, 0.85f, 0.4f, 1f),
            new Color(1f, 0.5f, 0.5f, 1f),
        };

        public void Bind(WorldConfig world, Action<WorldConfig> onPicked)
        {
            title.text = Loc.T(world.displayName);
            description.text = Loc.T(world.description);
            preview.sprite = world.previewSprite;
            preview.enabled = world.previewSprite != null;

            var tier = Mathf.Clamp(world.difficultyTier, 0, 2);
            enemiesValue.text = Loc.T(EnemiesKeys[tier]);
            lootValue.text = Loc.T(LootKeys[tier]);
            difficultyValue.text = Loc.T(DifficultyKeys[tier]);
            enemiesValue.color = lootValue.color = difficultyValue.color = TierColors[tier];

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onPicked(world));
        }
    }
}
