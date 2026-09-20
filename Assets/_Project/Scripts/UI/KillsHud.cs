using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>"KILLS 123": enemies killed in this run.</summary>
    public class KillsHud : MonoBehaviour
    {
        [SerializeField] private Text label;

        private int _shown = -1;

        private void Update()
        {
            var tracker = RunTracker.Instance;
            var kills = tracker != null ? tracker.Kills : 0;
            if (kills == _shown)
                return;
            _shown = kills;
            label.text = Loc.F("KILLS {0}", kills);
        }
    }
}
