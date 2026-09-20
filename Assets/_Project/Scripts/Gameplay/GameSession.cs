using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Resets time scale and ends the run (saving progress) when the player dies. Restart / menu live on the game-over screen.</summary>
    public class GameSession : MonoBehaviour
    {
        private void Start()
        {
            Time.timeScale = 1f;
            PlayerController.Instance.Health.Died += _ =>
            {
                if (RunTracker.Instance != null)
                    RunTracker.Instance.FinishRun();
            };
        }
    }
}
