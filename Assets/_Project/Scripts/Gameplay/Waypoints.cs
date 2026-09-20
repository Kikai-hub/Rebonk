using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Points of interest the HUD may point an arrow at. Set by the objects themselves, read by UI.</summary>
    public static class Waypoints
    {
        public static Transform Altar;
        public static Transform Portal;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Altar = null;
            Portal = null;
        }
    }
}
