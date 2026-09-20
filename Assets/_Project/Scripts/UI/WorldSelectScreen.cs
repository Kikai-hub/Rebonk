using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rebonk.UI
{
    /// <summary>Basic main menu: pick a world, start a run in it. (Final polish comes with the UI stage.)</summary>
    public class WorldSelectScreen : MonoBehaviour
    {
        [SerializeField] private WorldCatalog catalog;
        [SerializeField] private WorldCard[] cards;
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
        }

        private void OnPicked(WorldConfig world)
        {
            RunSettings.StartRun(world);
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
