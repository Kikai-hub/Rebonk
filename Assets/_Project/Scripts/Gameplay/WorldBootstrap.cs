using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// Runs before everything else in the game scene: makes sure a world is selected (falls back to the
    /// catalog's first world when the scene is started directly from the editor), resolves the saved
    /// character selection and rolls a fresh map seed.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class WorldBootstrap : MonoBehaviour
    {
        [SerializeField] private WorldCatalog catalog;
        [SerializeField] private MetaCatalog metaCatalog;
        [SerializeField] private Camera viewCamera;

        private void Awake()
        {
            if (RunSettings.World == null)
                RunSettings.StartRun(catalog.worlds[0]);
            else
                RunSettings.NewSeed();

            RunSettings.SetCharacter(metaCatalog != null ? metaCatalog.SelectedCharacter() : null);

            if (viewCamera != null)
                viewCamera.backgroundColor = RunSettings.World.backgroundColor;
        }
    }
}
