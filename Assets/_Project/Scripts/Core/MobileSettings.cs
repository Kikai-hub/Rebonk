using UnityEngine;

namespace Rebonk.Core
{
    /// <summary>Runtime defaults for phones: 60 FPS cap (Android defaults to 30), screen stays awake during a run.</summary>
    public static class MobileSettings
    {
        public const int TargetFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFrameRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }
    }
}
