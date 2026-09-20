using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Core
{
    public enum Language
    {
        English,
        Russian,
        Spanish,
        German,
        Portuguese
    }

    /// <summary>
    /// Tiny localization layer. The English text itself is the key: <c>Loc.T("PLAY")</c> returns the translation from
    /// <c>Resources/Localization/&lt;code&gt;.txt</c> (lines <c>English=&gt;Translation</c>, <c>\n</c> = line break) or the
    /// English text when the line is missing, so a missing translation never breaks the UI.
    /// </summary>
    public static class Loc
    {
        public static readonly string[] NativeNames = { "English", "Русский", "Español", "Deutsch", "Português" };
        private static readonly string[] Codes = { "en", "ru", "es", "de", "pt" };
        private const string PrefKey = "rebonk.language";

        public static event Action Changed;

        private static Dictionary<string, string> _table;
        private static Language _language;
        private static bool _ready;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _ready = false;
            _table = null;
            Changed = null;
        }

        public static Language Current
        {
            get { EnsureReady(); return _language; }
        }

        public static string NativeName(Language language) => NativeNames[(int)language];

        public static void Set(Language language)
        {
            EnsureReady();
            if (language == _language && _table != null)
                return;
            _language = language;
            Load();
            PlayerPrefs.SetInt(PrefKey, (int)language);
            PlayerPrefs.Save();
            Changed?.Invoke();
        }

        public static void Next() => Set((Language)(((int)Current + 1) % Codes.Length));

        /// <summary>Translates a UI string (the English text is the key). Empty strings and unknown texts pass through.</summary>
        public static string T(string english)
        {
            if (string.IsNullOrEmpty(english))
                return english;
            EnsureReady();
            string translated;
            return _table != null && _table.TryGetValue(english, out translated) ? translated : english;
        }

        /// <summary>Translates a format string and fills its <c>{0}</c>, <c>{1}</c> placeholders.</summary>
        public static string F(string english, params object[] args) => string.Format(T(english), args);

        private static void EnsureReady()
        {
            if (_ready)
                return;
            _ready = true;
            if (PlayerPrefs.HasKey(PrefKey))
                _language = (Language)Mathf.Clamp(PlayerPrefs.GetInt(PrefKey), 0, Codes.Length - 1);
            else
                _language = FromSystem(Application.systemLanguage);
            Load();
        }

        private static Language FromSystem(SystemLanguage system)
        {
            switch (system)
            {
                case SystemLanguage.Russian:
                case SystemLanguage.Ukrainian:
                case SystemLanguage.Belarusian:
                    return Language.Russian;
                case SystemLanguage.Spanish: return Language.Spanish;
                case SystemLanguage.German: return Language.German;
                case SystemLanguage.Portuguese: return Language.Portuguese;
                default: return Language.English;
            }
        }

        private static void Load()
        {
            _table = null;
            if (_language == Language.English)
                return;

            var asset = Resources.Load<TextAsset>("Localization/" + Codes[(int)_language]);
            if (asset == null)
            {
                Debug.LogWarning("Missing localization file for " + _language);
                return;
            }

            _table = new Dictionary<string, string>(512);
            foreach (var raw in asset.text.Split('\n'))
            {
                var line = raw.TrimEnd('\r');
                if (line.Length == 0 || line[0] == '#')
                    continue;
                var split = line.IndexOf("=>", StringComparison.Ordinal);
                if (split <= 0)
                    continue;
                var key = line.Substring(0, split).Trim().Replace("\\n", "\n");
                var value = line.Substring(split + 2).Trim().Replace("\\n", "\n");
                if (value.Length > 0)
                    _table[key] = value;
            }
        }
    }
}
