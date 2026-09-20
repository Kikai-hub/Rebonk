using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>Boss name and HP bar, visible only while a boss is alive.</summary>
    public class BossHealthBar : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image fill;
        [SerializeField] private Text nameText;

        private void Update()
        {
            var boss = BossController.Current;
            var visible = boss != null && boss.Health != null && !boss.Health.IsDead;
            if (root.activeSelf != visible)
                root.SetActive(visible);
            if (!visible)
                return;

            nameText.text = boss.DisplayName;
            fill.fillAmount = boss.Health.Max > 0f ? boss.Health.Current / boss.Health.Max : 0f;
        }
    }
}
