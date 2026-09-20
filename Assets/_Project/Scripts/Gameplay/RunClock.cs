using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Time spent in the current run (keeps counting across zones, pauses with the game and while dead).</summary>
    public class RunClock : MonoBehaviour
    {
        public static RunClock Instance { get; private set; }

        public float Elapsed { get; private set; }
        public float Minutes => Elapsed / 60f;

        private void Awake() => Instance = this;

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player != null && !player.Health.IsDead)
                Elapsed += Time.deltaTime;
        }

        /// <summary>Jump the clock (balance testing).</summary>
        [ContextMenu("Add 5 minutes (debug)")]
        public void DebugAddFiveMinutes() => Elapsed += 300f;

        public void SetElapsed(float seconds) => Elapsed = Mathf.Max(0f, seconds);
    }
}
