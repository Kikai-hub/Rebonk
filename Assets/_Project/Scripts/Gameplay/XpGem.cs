using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Pooled experience gem. Sits still until the player is within pickup radius, then flies to them.</summary>
    public class XpGem : MonoBehaviour
    {
        private const float StartSpeed = 6f;
        private const float Acceleration = 30f;
        private const float MaxSpeed = 18f;
        private const float CollectDistance = 0.3f;

        private static readonly System.Collections.Generic.List<XpGem> Active = new System.Collections.Generic.List<XpGem>(256);

        private int _value;
        private bool _attracted;
        private float _speed;
        private Action<XpGem> _release;

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        /// <summary>Removes every gem lying on the map (new zone = new map).</summary>
        public static void DespawnAll()
        {
            for (var i = Active.Count - 1; i >= 0; i--)
            {
                var gem = Active[i];
                var release = gem._release;
                gem._release = null;
                release?.Invoke(gem);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();

        public void Init(Vector2 position, int value, Action<XpGem> release)
        {
            transform.position = position;
            _value = value;
            _release = release;
            _attracted = false;
            _speed = StartSpeed;
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || _release == null)
                return;

            var toPlayer = (Vector2)player.transform.position - (Vector2)transform.position;
            var sqr = toPlayer.sqrMagnitude;
            var radius = player.Stats.PickupRadius;

            if (!_attracted && sqr <= radius * radius)
                _attracted = true;
            if (!_attracted)
                return;

            _speed = Mathf.Min(_speed + Acceleration * Time.deltaTime, MaxSpeed);
            var dist = Mathf.Sqrt(sqr);
            var step = _speed * Time.deltaTime;

            if (dist <= CollectDistance || step >= dist)
            {
                Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.GemPickup);
                player.Level.AddXp(_value);
                var release = _release;
                _release = null;
                release(this);
                return;
            }

            transform.position += (Vector3)(toPlayer / dist * step);
        }
    }
}
