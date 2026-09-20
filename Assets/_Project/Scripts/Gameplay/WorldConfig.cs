using System;
using UnityEngine;

namespace Rebonk.Gameplay
{
    [Serializable]
    public class WorldProp
    {
        public Sprite sprite;
        [Min(0f)] public float weight = 1f;
        [Tooltip("Obstacles only: the player cannot walk into this circle (world units).")]
        [Min(0f)] public float collisionRadius = 0.7f;
        public float scale = 1f;
    }

    /// <summary>
    /// Everything that makes a world different: look (tiles, obstacles, decorations, background),
    /// generation density, enemy roster and boss. New worlds are new assets, not new code.
    /// </summary>
    [CreateAssetMenu(menuName = "Rebonk/World Config", fileName = "World_")]
    public class WorldConfig : ScriptableObject
    {
        public string displayName = "World";
        [TextArea] public string description;
        public Sprite previewSprite;
        public Color backgroundColor = new Color(0.2f, 0.25f, 0.3f);

        [Header("Ground tiles (16x16 sprites)")]
        public Sprite[] groundTiles;
        [Tooltip("Alternate ground drawn in noise patches.")]
        public Sprite[] patchTiles;
        [Range(0f, 1f)] public float patchThreshold = 0.62f;
        [Tooltip("Lower = bigger patches.")]
        public float patchNoiseScale = 0.06f;

        [Header("Obstacles (solid for the player)")]
        public WorldProp[] obstacles;
        [Tooltip("Placement attempts per chunk; spacing rules reject some, so the real count is lower.")]
        public int obstacleAttemptsPerChunk = 14;
        [Tooltip("Extra gap between any two obstacles. Keep >= 1.2 so a path always exists (player is ~0.8 wide).")]
        public float obstacleGap = 1.4f;

        [Header("Decorations (walkable)")]
        public WorldProp[] decorations;
        public int decorationsPerChunk = 18;

        [Header("Enemies and boss")]
        public SpawnEntry[] enemies;
        [Header("Music (empty = default from the Sound Bank)")]
        public AudioClip zoneMusic;
        public AudioClip survivalMusic;
        public AudioClip bossMusic;

        public Enemy bossPrefab;
        public EnemyData bossData;
        [Tooltip("World-specific difficulty on top of DifficultyConfig.")]
        public float hpMultiplier = 1f;
        public float damageMultiplier = 1f;
    }
}
