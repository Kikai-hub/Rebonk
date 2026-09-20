using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Balancing tool (editor only): runs the balance bot over a queue of configs, appending results to a file.</summary>
[InitializeOnLoad]
public static class BotRunner
{
    private static readonly string OutFile = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp", "bot_results.txt");
    private static double _waitUntil;

    static BotRunner() => EditorApplication.update += Tick;

    public static void Start(string queue)
    {
        EditorPrefs.SetString("botq.queue", queue);   // "world:char:runs;..." flattened to "world:char" tokens separated by |
        EditorPrefs.SetInt("botq.active", 1);
        File.WriteAllText(OutFile, "");
    }

    private static void Tick()
    {
        if (EditorPrefs.GetInt("botq.active", 0) != 1 || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;
        if (EditorApplication.timeSinceStartup < _waitUntil)
            return;

        if (EditorApplication.isPlaying)
        {
            var result = EditorPrefs.GetString("bot.result", "");
            if (result.Length == 0)
                return;
            File.AppendAllText(OutFile, result + "\n");
            EditorPrefs.SetString("bot.result", "");
            EditorApplication.isPlaying = false;
            _waitUntil = EditorApplication.timeSinceStartup + 2;
            return;
        }

        var queue = EditorPrefs.GetString("botq.queue", "");
        if (queue.Length == 0)
        {
            EditorPrefs.SetInt("botq.active", 0);
            EditorPrefs.SetInt("bot.enabled", 0);
            File.AppendAllText(OutFile, "DONE\n");
            return;
        }

        var parts = queue.Split('|');
        var next = parts[0].Split(':');
        EditorPrefs.SetString("botq.queue", string.Join("|", parts, 1, parts.Length - 1));
        EditorPrefs.SetInt("bot.world", int.Parse(next[0]));
        EditorPrefs.SetString("bot.char", next.Length > 1 ? next[1] : "");
        EditorPrefs.SetInt("bot.unlockAll", next.Length > 2 && next[2] == "u" ? 1 : 0);
        EditorPrefs.SetInt("bot.enabled", 1);
        EditorPrefs.SetString("bot.result", "");
        Rebonk.Gameplay.SaveSystem.DeleteSave();
        PlayerPrefs.DeleteAll();
        EditorApplication.isPlaying = true;
        _waitUntil = EditorApplication.timeSinceStartup + 3;
    }
}
