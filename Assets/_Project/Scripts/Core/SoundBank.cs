using System;
using UnityEngine;

namespace Rebonk.Core
{
    public enum SfxId
    {
        UiClick,
        PlayerHurt,
        EnemyHit,
        EnemyDie,
        GemPickup,
        LevelUp,
        WeaponShot,
        WeaponMelee,
        WeaponArea,
        WeaponBoomerang,
        Evolution,
        SpiritsStart,
        BossSpawn,
        BossDefeated,
        PortalOpen,
        ZoneCleared,
        TotemDone,
        AltarActivate,
        GameOver
    }

    [Serializable]
    public class SfxEntry
    {
        public SfxId id;
        [Tooltip("Several clips = a random one each time.")]
        public AudioClip[] clips;
        [Range(0f, 1f)] public float volume = 1f;
        [Tooltip("Random pitch spread, e.g. 0.1 = +-10%.")]
        [Range(0f, 0.5f)] public float pitchJitter = 0.08f;
        [Tooltip("The same sound will not start again sooner than this (keeps swarms from turning into noise).")]
        public float minInterval = 0.04f;
    }

    /// <summary>
    /// All sounds and music tracks in one asset (Resources/Audio/SoundBank). To replace a placeholder,
    /// overwrite the audio file or drop a new clip into the slot; no code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Rebonk/Sound Bank", fileName = "SoundBank")]
    public class SoundBank : ScriptableObject
    {
        public SfxEntry[] sfx;
        [Header("Music")]
        public AudioClip menuMusic;
        [Tooltip("Used when the world has no track of its own.")]
        public AudioClip defaultZoneMusic;
        public AudioClip survivalMusic;
        public AudioClip bossMusic;

        public SfxEntry Find(SfxId id)
        {
            if (sfx == null)
                return null;
            foreach (var e in sfx)
                if (e.id == id)
                    return e;
            return null;
        }
    }
}
