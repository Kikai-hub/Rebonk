using Rebonk.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Put on a static label: the text authored in the scene is the English key and is translated on start and on language change.</summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        private Text _text;
        private string _key;

        private void Awake()
        {
            _text = GetComponent<Text>();
            _key = _text.text;
        }

        private void OnEnable()
        {
            Loc.Changed += Apply;
            Apply();
        }

        private void OnDisable() => Loc.Changed -= Apply;

        private void Apply()
        {
            if (_text != null)
                _text.text = Loc.T(_key);
        }
    }
}
