using System;
using System.IO;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// The single save approach for the whole project: one JSON file in the platform's persistent data folder.
    /// Access the profile through <see cref="Data"/>; call <see cref="Save"/> after changing it.
    /// </summary>
    public static class SaveSystem
    {
        private static SaveData _data;

        public static string FilePath => Path.Combine(Application.persistentDataPath, "rebonk_save.json");

        public static SaveData Data
        {
            get
            {
                if (_data == null)
                    Load();
                return _data;
            }
        }

        public static void Load()
        {
            _data = null;
            try
            {
                if (File.Exists(FilePath))
                    _data = JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not read the save file, starting fresh: " + e.Message);
            }

            if (_data == null)
                _data = new SaveData();
        }

        public static void Save()
        {
            try
            {
                var tmp = FilePath + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(Data, true));
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
                File.Move(tmp, FilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not write the save file: " + e.Message);
            }
        }

        /// <summary>Erases all saved progress (file and memory). Intended for a "reset progress" option and tests.</summary>
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(FilePath))
                    File.Delete(FilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Could not delete the save file: " + e.Message);
            }
            _data = null;
        }

        /// <summary>Forgets the in-memory copy so the next access re-reads the file (also used to simulate a fresh launch).</summary>
        public static void ForgetInMemory() => _data = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => _data = null;
    }
}
