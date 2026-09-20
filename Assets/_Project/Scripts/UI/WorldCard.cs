using Rebonk.Core;
using System;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>One selectable world on the world-select screen.</summary>
    public class WorldCard : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image preview;
        [SerializeField] private Text title;
        [SerializeField] private Text description;

        public void Bind(WorldConfig world, Action<WorldConfig> onPicked)
        {
            title.text = Loc.T(world.displayName);
            description.text = Loc.T(world.description);
            preview.sprite = world.previewSprite;
            preview.enabled = world.previewSprite != null;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onPicked(world));
        }
    }
}
