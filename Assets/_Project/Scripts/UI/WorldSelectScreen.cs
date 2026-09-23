using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>World-select screen: pick a world, start a run in it directly.</summary>
    public class WorldSelectScreen : MonoBehaviour
    {
        [SerializeField] private WorldCatalog catalog;
        [SerializeField] private WorldCard[] cards;
        [SerializeField] private Text unlockedCounter;
        [SerializeField] private string gameSceneName = "Game";

        private void Start()
        {
            Time.timeScale = 1f;
            Loc.Changed += Bind;
            Bind();
        }

        private void OnDestroy() => Loc.Changed -= Bind;

        private void Bind()
        {
            for (var i = 0; i < cards.Length; i++)
            {
                var has = i < catalog.worlds.Count;
                cards[i].gameObject.SetActive(has);
                if (has)
                    cards[i].Bind(catalog.worlds[i], OnPicked);
            }

            if (unlockedCounter != null)
                unlockedCounter.text = Loc.F("Unlocked {0} / {1}", catalog.worlds.Count, catalog.worlds.Count);
        }

        private void OnPicked(WorldConfig world)
        {
            RunSettings.StartRun(world);
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
