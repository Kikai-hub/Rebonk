using Rebonk.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Rebonk.UI
{
    /// <summary>Put on any button to play the click sound when it is pressed.</summary>
    public class UiClick : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData) => Sound.Play(SfxId.UiClick);
    }
}
