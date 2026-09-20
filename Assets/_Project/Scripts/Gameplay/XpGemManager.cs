using Rebonk.Core;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Drops a pooled XP gem where an enemy dies.</summary>
    public class XpGemManager : MonoBehaviour
    {
        [SerializeField] private XpGem gemPrefab;
        [SerializeField] private int prewarm = 60;

        private ComponentPool<XpGem> _pool;

        private void Awake()
        {
            _pool = new ComponentPool<XpGem>(gemPrefab, transform, prewarm, 2000);
        }

        private void OnEnable() => Enemy.Killed += OnEnemyKilled;

        private void OnDisable() => Enemy.Killed -= OnEnemyKilled;

        private void OnEnemyKilled(Enemy enemy)
        {
            if (enemy.XpValue <= 0)
                return;

            _pool.Get().Init(enemy.transform.position, enemy.XpValue, _pool.Release);
        }
    }
}
