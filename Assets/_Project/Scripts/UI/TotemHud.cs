using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>"TOTEMS 3/17": activated / total on the current map.</summary>
    public class TotemHud : MonoBehaviour
    {
        [SerializeField] private Text label;

        private void Update()
        {
            label.text = Loc.F("TOTEMS {0}/{1}", Totem.UsedCount, Totem.TotalCount);
        }
    }
}
