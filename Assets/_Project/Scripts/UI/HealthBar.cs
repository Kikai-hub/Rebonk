using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image fill;

        private void Start()
        {
            var health = PlayerController.Instance.Health;
            health.Changed += OnChanged;
            OnChanged(health.Current, health.Max);
        }

        private void OnChanged(float current, float max)
        {
            fill.fillAmount = max > 0f ? current / max : 0f;
        }
    }
}
