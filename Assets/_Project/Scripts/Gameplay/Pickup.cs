using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public enum PickupKind
    {
        Magnet,
        SpeedBoost,
        HotEgg
    }

    /// <summary>Rare item dropped by an enemy. Lies on the map until the player walks onto it (no pickup-radius magnet).</summary>
    public class Pickup : MonoBehaviour
    {
        public static readonly List<Pickup> Active = new List<Pickup>(16);

        [SerializeField] private SpriteRenderer body;
        [SerializeField] private float collectDistance = 0.8f;
        [SerializeField] private float bobHeight = 0.12f;
        [SerializeField] private float bobSpeed = 3f;

        private Action<Pickup> _collected;
        private Vector2 _basePosition;
        private float _phase;

        public PickupKind Kind { get; private set; }

        private void OnEnable() => Active.Add(this);

        private void OnDisable() => Active.Remove(this);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active.Clear();

        public void Init(PickupKind kind, Sprite sprite, Vector2 position, Action<Pickup> collected)
        {
            Kind = kind;
            _collected = collected;
            _basePosition = position;
            _phase = UnityEngine.Random.value * Mathf.PI * 2f;
            transform.position = position;
            if (body != null)
                body.sprite = sprite;
        }

        private void Update()
        {
            if (_collected == null)
                return;

            // Gentle bob so the item stands out from gems and decor.
            transform.position = _basePosition + Vector2.up * (Mathf.Sin(Time.time * bobSpeed + _phase) * bobHeight);

            var player = PlayerController.Instance;
            if (player == null || player.Health.IsDead)
                return;

            if (((Vector2)player.transform.position - _basePosition).sqrMagnitude > collectDistance * collectDistance)
                return;

            var collected = _collected;
            _collected = null;
            collected(this);
        }
    }
}
