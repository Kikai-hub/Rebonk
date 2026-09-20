#if UNITY_EDITOR
using System.Collections;
using System.IO;
using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Rebonk.UI
{
    /// <summary>Balancing aid (editor only, see DocsProject/Balance_Tools.md): measures each weapon's real damage output against stationary dummies.</summary>
    public class WeaponBench : MonoBehaviour
    {
        private static readonly string OutFile = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Temp", "bench_results.txt");
        private float _sum;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            if (EditorPrefs.GetInt("bench.enabled", 0) == 1 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game")
                new GameObject("WeaponBench").AddComponent<WeaponBench>();
        }

        private void OnEnable() => Enemy.Damaged += OnDamaged;
        private void OnDisable() => Enemy.Damaged -= OnDamaged;
        private void OnDamaged(float amount) => _sum += amount;

        private IEnumerator Start()
        {
            yield return null;
            yield return null;
            var player = PlayerController.Instance;
            foreach (var s in FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None)) s.enabled = false;
            foreach (var s in FindObjectsByType<SpiritDirector>(FindObjectsSortMode.None)) s.enabled = false;
            foreach (var w in FindObjectsByType<Weapon>(FindObjectsSortMode.None)) Destroy(w.gameObject);
            foreach (var e in Enemy.Active.ToArray()) e.Despawn();

            var meta = AssetDatabase.LoadAssetAtPath<MetaCatalog>("Assets/_Project/ScriptableObjects/MetaCatalog.asset");
            var dummyData = AssetDatabase.LoadAssetAtPath<EnemyData>("Assets/_Project/ScriptableObjects/Enemies/Enemy_Tank.asset");
            var dummyPrefab = AssetDatabase.LoadAssetAtPath<Enemy>("Assets/_Project/Prefabs/Enemy_Tank.prefab");
            var weapons = new System.Collections.Generic.List<WeaponData>(meta.Weapons());
            foreach (var evo in meta.pool.evolutions)
                if (evo.result != null && !weapons.Contains(evo.result)) weapons.Add(evo.result);

            Time.timeScale = EditorPrefs.GetFloat("bench.timescale", 5f);
            File.WriteAllText(OutFile, "");
            const float seconds = 6f;
            var types = EditorPrefs.GetString("bench.types", "");

            foreach (var data in weapons)
            {
                if (data.weaponPrefab == null) continue;
                var only = EditorPrefs.GetString("bench.only", "");
                if (only.Length > 0 && !(","+only+",").Contains(","+data.name.Replace("Weapon_","")+",")) continue;
                if (types.Length > 0 && !(","+types+",").Contains(","+data.weaponPrefab.GetType().Name.Replace("Weapon","")+",")) continue;
                var line = data.name.Replace("Weapon_", "") + "|" + data.weaponPrefab.GetType().Name.Replace("Weapon", "") + "|" + data.category.ToString().Substring(0, 1);
                foreach (var mode in new[] { "single", "crowd" })
                {
                    foreach (var level in new[] { 1, 3, data.MaxLevel })
                    {
                        if (mode == "single" && level != data.MaxLevel) continue;
                        if (level == 3) continue;
                        SpawnDummies(dummyPrefab, dummyData, player, mode == "single" ? 1 : 24);
                        var weapon = Instantiate(data.weaponPrefab, player.transform);
                        weapon.Init(data, player);
                        weapon.SetLevel(level);
                        yield return null;
                        _sum = 0f;
                        var t0 = Time.time;
                        while (Time.time - t0 < seconds)
                        {
                            player.Health.Invulnerable = true;
                            yield return null;
                        }
                        line += "|" + mode[0] + level + "=" + (_sum / seconds).ToString("0.0");
                        Destroy(weapon.gameObject);
                        foreach (var e in Enemy.Active.ToArray()) Destroy(e.gameObject);
                        yield return null;
                        yield return null;
                    }
                }
                File.AppendAllText(OutFile, line + "\n");
            }

            File.AppendAllText(OutFile, "DONE\n");
            EditorPrefs.SetInt("bench.enabled", 0);
            EditorApplication.isPlaying = false;
        }

        private static void SpawnDummies(Enemy prefab, EnemyData data, PlayerController player, int count)
        {
            var rng = new System.Random(7);
            for (var i = 0; i < count; i++)
            {
                Vector2 pos;
                if (count == 1)
                    pos = (Vector2)player.transform.position + new Vector2(1.6f, 0f);
                else
                {
                    var angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                    var r = 1.5f + (float)rng.NextDouble() * 3f;
                    pos = (Vector2)player.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
                }
                var e = Instantiate(prefab, pos, Quaternion.identity);
                e.Init(data, x => Destroy(x.gameObject), new EnemyScale { Hp = 100000f, Speed = 0f, Damage = 0f });
            }
        }
    }
}
#endif
