using UnityEngine;

namespace Rebonk.Core
{
    /// <summary>
    /// The one place to play sounds: <c>Sound.Play(SfxId.EnemyDie)</c> and <c>Sound.PlayMusic(clip)</c>.
    /// Lives on a hidden object that survives scene loads; volumes follow <see cref="GameSettings"/>.
    /// </summary>
    public class Sound : MonoBehaviour
    {
        private const int SfxVoices = 16;
        private const float DuckWhenPaused = 0.4f;

        private static Sound _instance;

        private SoundBank _bank;
        private AudioSource[] _voices;
        private int _nextVoice;
        private readonly float[] _lastPlayed = new float[System.Enum.GetValues(typeof(SfxId)).Length];

        private AudioSource[] _music;
        private int _activeMusic;
        private AudioClip _musicClip;
        private float _fadeSeconds = 1f;
        private float _fadeT = 1f;
        private float _duck = 1f;

        public static SoundBank Bank => Ensure()._bank;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() => Ensure();

        private static Sound Ensure()
        {
            if (_instance != null)
                return _instance;

            var go = new GameObject("Sound");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<Sound>();
            _instance.Setup();
            return _instance;
        }

        private void Setup()
        {
            _bank = Resources.Load<SoundBank>("Audio/SoundBank");

            // The only listener in the game, living across scenes (all audio is 2D, so its position does not matter).
            gameObject.AddComponent<AudioListener>();

            _voices = new AudioSource[SfxVoices];
            for (var i = 0; i < SfxVoices; i++)
                _voices[i] = NewSource(false);

            _music = new[] { NewSource(true), NewSource(true) };
        }

        private AudioSource NewSource(bool loop)
        {
            var s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = loop;
            s.spatialBlend = 0f;
            return s;
        }

        public static void Play(SfxId id, float volumeScale = 1f)
        {
            var self = Ensure();
            var entry = self._bank != null ? self._bank.Find(id) : null;
            if (entry == null || entry.clips == null || entry.clips.Length == 0)
                return;

            var now = Time.unscaledTime;
            var index = (int)id;
            if (now - self._lastPlayed[index] < entry.minInterval)
                return;
            self._lastPlayed[index] = now;

            var clip = entry.clips[entry.clips.Length == 1 ? 0 : Random.Range(0, entry.clips.Length)];
            if (clip == null)
                return;

            var voice = self._voices[self._nextVoice];
            self._nextVoice = (self._nextVoice + 1) % SfxVoices;
            voice.pitch = 1f + Random.Range(-entry.pitchJitter, entry.pitchJitter);
            voice.clip = clip;
            voice.volume = entry.volume * volumeScale * GameSettings.SfxVolume;
            voice.Play();
        }

        /// <summary>Crossfades to the clip (nothing happens if it is already playing; null fades the music out).</summary>
        public static void PlayMusic(AudioClip clip, float fadeSeconds = 1.2f)
        {
            var self = Ensure();
            if (clip == self._musicClip)
                return;

            self._musicClip = clip;
            self._fadeSeconds = Mathf.Max(0.01f, fadeSeconds);
            self._fadeT = 0f;
            self._activeMusic = 1 - self._activeMusic;

            var incoming = self._music[self._activeMusic];
            incoming.clip = clip;
            incoming.volume = 0f;
            if (clip != null)
                incoming.Play();
        }

        private void Update()
        {
            var targetDuck = Time.timeScale <= 0.01f ? DuckWhenPaused : 1f;
            _duck = Mathf.MoveTowards(_duck, targetDuck, Time.unscaledDeltaTime * 3f);

            _fadeT = Mathf.Min(1f, _fadeT + Time.unscaledDeltaTime / _fadeSeconds);
            var master = GameSettings.MusicVolume * _duck;
            for (var i = 0; i < _music.Length; i++)
            {
                var source = _music[i];
                var share = i == _activeMusic ? _fadeT : 1f - _fadeT;
                source.volume = share * master;
                if (share <= 0f && source.isPlaying)
                    source.Stop();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _instance = null;
    }
}
