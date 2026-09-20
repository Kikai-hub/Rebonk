using Rebonk.Core;
using UnityEngine;

namespace Rebonk.UI
{
    /// <summary>Starts the menu music when the main menu scene opens.</summary>
    public class MenuAudio : MonoBehaviour
    {
        private void Start()
        {
            var bank = Sound.Bank;
            Sound.PlayMusic(bank != null ? bank.menuMusic : null);
        }
    }
}
