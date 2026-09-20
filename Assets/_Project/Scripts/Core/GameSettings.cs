using System;
using UnityEngine;

namespace Rebonk.Core
{
    /// <summary>Player preferences stored in PlayerPrefs (separate from the progress save, so resetting progress keeps them).</summary>
    public static class GameSettings
    {
        private const string MusicKey = "rebonk.music";
        private const string SfxKey = "rebonk.sfx";

        public static event Action Changed;

        public static float MusicVolume
        {
            get => PlayerPrefs.GetFloat(MusicKey, 0.8f);
            set { PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value)); Changed?.Invoke(); }
        }

        public static float SfxVolume
        {
            get => PlayerPrefs.GetFloat(SfxKey, 1f);
            set { PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value)); Changed?.Invoke(); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Changed = null;
    }
}
