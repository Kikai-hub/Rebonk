using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Swings an arc at the nearest enemy in range; damages every enemy inside the arc (360 = full spin).</summary>
    public class MeleeWeapon : Weapon
    {
        [SerializeField] private float arcDegrees = 120f;
        [SerializeField] private float visualDuration = 0.12f;
        [SerializeField] private SpriteRenderer slash;
        [Tooltip("Optional second slash drawn on the opposite side when 'Both Sides' is on.")]
        [SerializeField] private SpriteRenderer slashBack;
        [Tooltip("Also swings (and hits) the opposite direction.")]
        [SerializeField] private bool bothSides;
        [Tooltip("World distance from the sprite pivot to its outer edge at scale 1 (wedge sprite: 2, ring sprite: 1).")]
        [SerializeField] private float slashReach = 2f;
        [Tooltip("Ring-style visuals are centered on the player and not rotated toward the target.")]
        [SerializeField] private bool slashCentered;

        private const float WedgeReferenceArc = 120f;

        private float _timer;
        private float _hideAt;

        private void Awake()
        {
            if (slash != null)
                slash.enabled = false;
            if (slashBack != null)
                slashBack.enabled = false;
        }

        private void Update()
        {
            if (Time.time >= _hideAt)
            {
                if (slash != null && slash.enabled) slash.enabled = false;
                if (slashBack != null && slashBack.enabled) slashBack.enabled = false;
            }

            if (!Ready)
                return;

            _timer -= Time.deltaTime;
            if (_timer > 0f)
                return;

            Vector2 origin = transform.position;
            if (!EnemyQuery.TryGetNearest(origin, Area, out var dir))
                return;

            _timer = Cooldown;
            Swing(origin, dir);
        }

        private void Swing(Vector2 origin, Vector2 dir)
        {
            Rebonk.Core.Sound.Play(Rebonk.Core.SfxId.WeaponMelee);
            var damage = Damage;
            var halfArc = arcDegrees * 0.5f;
            var range = Area;
            var rangeSqr = range * range;

            var list = Enemy.Active;
            for (var i = list.Count - 1; i >= 0; i--)
            {
                var enemy = list[i];
                var to = (Vector2)enemy.transform.position - origin;
                if (to.sqrMagnitude > rangeSqr)
                    continue;

                if (to.sqrMagnitude > 0.0001f)
                {
                    var inFront = Vector2.Angle(dir, to) <= halfArc;
                    var inBack = bothSides && Vector2.Angle(-dir, to) <= halfArc;
                    if (!inFront && !inBack)
                        continue;
                }

                enemy.Health.TakeDamage(damage);
            }

            ShowSlash(slash, dir, range);
            if (bothSides)
                ShowSlash(slashBack, -dir, range);
            _hideAt = Time.time + visualDuration;
        }

        private void ShowSlash(SpriteRenderer sr, Vector2 dir, float range)
        {
            if (sr == null)
                return;

            var scale = range / slashReach;
            if (slashCentered)
            {
                sr.transform.localRotation = Quaternion.identity;
                sr.transform.localScale = new Vector3(scale, scale, 1f);
            }
            else
            {
                var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                sr.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
                // Narrow arcs get a narrower wedge so the visual roughly matches the hit area.
                sr.transform.localScale = new Vector3(scale, scale * Mathf.Clamp(arcDegrees / WedgeReferenceArc, 0.3f, 2f), 1f);
            }

            sr.enabled = true;
        }
    }
}
