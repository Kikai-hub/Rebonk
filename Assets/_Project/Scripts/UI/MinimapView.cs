using System.Collections.Generic;
using Rebonk.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// One rendered map surface: reads the shared <see cref="MinimapModel"/> texture and crops it to a
    /// square window (world <paramref name="center"/> ± <paramref name="halfSize"/>), plus draws marker
    /// dots for the player, altar/portal, boss and totems, and a capped, pooled set of nearby enemies.
    /// Used for both the small always-on thumbnail (player-centred, small radius) and the tap-to-open
    /// full map (arena-centred, radius = the whole zone) — same component, different crop.
    /// </summary>
    public class MinimapView : MonoBehaviour
    {
        [SerializeField] private RawImage image;
        [SerializeField] private Image playerMarker;
        [SerializeField] private Image altarMarker;
        [SerializeField] private Image portalMarker;
        [SerializeField] private Image bossMarker;
        [Tooltip("Disabled template, cloned into a pool as needed. Parent it under the same RawImage.")]
        [SerializeField] private Image totemMarkerTemplate;
        [SerializeField] private Image enemyMarkerTemplate;
        [SerializeField] private int maxEnemyMarkers = 40;

        private static readonly Color TotemAvailable = new Color(0.1f, 0.6f, 0.95f, 1f);
        private static readonly Color TotemDone = new Color(0.5f, 0.5f, 0.55f, 0.8f);

        private readonly List<Image> _totemMarkers = new List<Image>();
        private readonly List<Image> _enemyMarkers = new List<Image>();
        private RectTransform _rect;

        private void Awake()
        {
            _rect = image.rectTransform;
            if (totemMarkerTemplate != null) totemMarkerTemplate.gameObject.SetActive(false);
            if (enemyMarkerTemplate != null) enemyMarkerTemplate.gameObject.SetActive(false);
        }

        /// <summary>Redraws the crop window and every marker for the given world-space centre/radius.</summary>
        public void Refresh(Vector2 center, float halfSize)
        {
            var full = MinimapModel.ArenaHalfSize;
            if (full <= 0f || MinimapModel.Texture == null)
            {
                image.enabled = false;
                return;
            }
            image.enabled = true;
            if (image.texture != MinimapModel.Texture)
                image.texture = MinimapModel.Texture;

            var half01 = Mathf.Clamp(halfSize / (full * 2f), 0.001f, 0.5f);
            var cu = 0.5f + center.x / (full * 2f);
            var cv = 0.5f + center.y / (full * 2f);
            image.uvRect = new Rect(cu - half01, cv - half01, half01 * 2f, half01 * 2f);

            var pixelHalf = _rect.rect.width * 0.5f;
            Vector2 ToLocal(Vector2 world) => (world - center) / halfSize * pixelHalf;
            bool Inside(Vector2 local) => Mathf.Abs(local.x) <= pixelHalf && Mathf.Abs(local.y) <= pixelHalf;
            // A marker only shows once the player has actually uncovered that spot's fog, not merely because
            // it happens to fall within the crop window (otherwise totems/enemies would "pop in" through the fog).
            bool Visible(Vector2 world, Vector2 local) => Inside(local) && MinimapModel.IsRevealed(world);

            var player = PlayerController.Instance;
            SetMarker(playerMarker, player != null ? ToLocal(player.transform.position) : Vector2.zero, player != null);

            var boss = BossController.Current;
            if (boss != null)
            {
                var l = ToLocal(boss.transform.position);
                SetMarker(bossMarker, l, Visible(boss.transform.position, l));
            }
            else
            {
                HideMarker(bossMarker);
            }

            // Portal and altar share one spot (the portal spawns where the altar stood): show whichever is current.
            var portalT = Waypoints.Portal;
            var altar = Altar.Current;
            if (portalT != null)
            {
                var l = ToLocal(portalT.position);
                SetMarker(portalMarker, l, Visible(portalT.position, l));
                HideMarker(altarMarker);
            }
            else if (altar != null && altar.Discovered)
            {
                var l = ToLocal(altar.transform.position);
                SetMarker(altarMarker, l, Visible(altar.transform.position, l));
                HideMarker(portalMarker);
            }
            else
            {
                HideMarker(altarMarker);
                HideMarker(portalMarker);
            }

            RefreshTotems(ToLocal, Visible);
            RefreshEnemies(ToLocal, Visible);
        }

        private void RefreshTotems(System.Func<Vector2, Vector2> toLocal, System.Func<Vector2, Vector2, bool> visible)
        {
            if (totemMarkerTemplate == null)
                return;
            EnsurePool(_totemMarkers, totemMarkerTemplate, Totem.All.Count);
            for (var i = 0; i < _totemMarkers.Count; i++)
            {
                if (i >= Totem.All.Count || Totem.All[i] == null)
                {
                    _totemMarkers[i].gameObject.SetActive(false);
                    continue;
                }
                var t = Totem.All[i];
                var local = toLocal(t.transform.position);
                var show = visible(t.transform.position, local);
                _totemMarkers[i].gameObject.SetActive(show);
                if (!show)
                    continue;
                _totemMarkers[i].rectTransform.anchoredPosition = local;
                _totemMarkers[i].color = t.IsActivated ? TotemDone : TotemAvailable;
            }
        }

        private void RefreshEnemies(System.Func<Vector2, Vector2> toLocal, System.Func<Vector2, Vector2, bool> visible)
        {
            if (enemyMarkerTemplate == null)
                return;
            EnsurePool(_enemyMarkers, enemyMarkerTemplate, maxEnemyMarkers);

            var shown = 0;
            var list = Enemy.Active;
            for (var i = 0; i < list.Count && shown < _enemyMarkers.Count; i++)
            {
                var e = list[i];
                if (e == null || e.Data == null || e.Data.behavior == EnemyBehavior.Boss)
                    continue;
                var local = toLocal(e.transform.position);
                if (!visible(e.transform.position, local))
                    continue;
                var img = _enemyMarkers[shown++];
                img.gameObject.SetActive(true);
                img.rectTransform.anchoredPosition = local;
            }
            for (var i = shown; i < _enemyMarkers.Count; i++)
                _enemyMarkers[i].gameObject.SetActive(false);
        }

        private static void EnsurePool(List<Image> pool, Image template, int count)
        {
            while (pool.Count < count)
            {
                var clone = Instantiate(template, template.transform.parent);
                clone.gameObject.SetActive(false);
                pool.Add(clone);
            }
        }

        private static void SetMarker(Image img, Vector2 local, bool visible)
        {
            if (img == null)
                return;
            img.gameObject.SetActive(visible);
            if (visible)
                img.rectTransform.anchoredPosition = local;
        }

        private static void HideMarker(Image img)
        {
            if (img != null)
                img.gameObject.SetActive(false);
        }
    }
}
