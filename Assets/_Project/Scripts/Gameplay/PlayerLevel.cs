using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Run-level experience and level tracking. Curve: 5 * level^1.4 XP to leave a level.</summary>
    public class PlayerLevel : MonoBehaviour
    {
        [SerializeField] private float baseXp = 5f;
        [SerializeField] private float growth = 1.4f;

        private PlayerStats _stats;
        private float _fraction;

        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int XpToNext => Mathf.Max(1, Mathf.RoundToInt(baseXp * Mathf.Pow(Level, growth)));

        /// <summary>Fired on any XP/level change.</summary>
        public event Action Changed;
        /// <summary>Fired once per gained level, with the new level.</summary>
        public event Action<int> LeveledUp;

        private void Awake() => _stats = GetComponent<PlayerStats>();

        public void AddXp(int amount)
        {
            // Character XP bonus; the remainder is carried so small gems are not lost to rounding.
            var scaled = amount * (_stats != null ? _stats.XpMultiplier : 1f) + _fraction; // XpMultiplier = character x items x blessings
            var whole = Mathf.FloorToInt(scaled);
            _fraction = scaled - whole;

            Xp += whole;
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                LeveledUp?.Invoke(Level);
            }
            Changed?.Invoke();
        }
    }
}
