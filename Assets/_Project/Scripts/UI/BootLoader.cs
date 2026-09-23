using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rebonk.UI
{
    /// <summary>
    /// First scene of the build: shows the loading screen while the main menu loads in the background,
    /// for at least a moment so it never just flickers.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private string nextScene = "MainMenu";
        [SerializeField] private float minimumSeconds = 7f;

        private IEnumerator Start()
        {
            var started = Time.realtimeSinceStartup;
            var load = SceneManager.LoadSceneAsync(nextScene);
            load.allowSceneActivation = false;

            // Async loading stops at 0.9 until activation is allowed.
            while (load.progress < 0.9f || Time.realtimeSinceStartup - started < minimumSeconds)
                yield return null;

            load.allowSceneActivation = true;
        }
    }
}
