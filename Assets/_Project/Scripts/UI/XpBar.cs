using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    public class XpBar : MonoBehaviour
    {
        [SerializeField] private Image fill;
        [SerializeField] private Text levelLabel;

        private PlayerLevel _level;

        private void Start()
        {
            _level = PlayerController.Instance.Level;
            _level.Changed += Refresh;
            Refresh();
        }

        private void OnDestroy()
        {
            if (_level != null)
                _level.Changed -= Refresh;
        }

        private void Refresh()
        {
            fill.fillAmount = (float)_level.Xp / _level.XpToNext;
            levelLabel.text = Loc.F("Lv {0}", _level.Level);
        }
    }
}
