#if UNITY_EDITOR
using Rebonk.Core;
using Rebonk.Gameplay;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Rebonk.UI
{
    /// <summary>
    /// Balancing aid (editor only, see DocsProject/Balance_Tools.md): plays a run by itself (flees enemies, grabs gems, picks random cards)
    /// at high time scale and logs a line per game minute. Configured through EditorPrefs bot.*.
    /// </summary>
    public class BalanceBot : MonoBehaviour
    {
        private float _nextLog = 60f;
        private float _nextGem;
        private Vector2 _gemDir;
        private float _nextPick;
        private bool _done;
        private bool _bossCalled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            if (EditorPrefs.GetInt("bot.enabled", 0) != 1)
                return;
            var worlds = AssetDatabase.LoadAssetAtPath<WorldCatalog>("Assets/_Project/ScriptableObjects/WorldCatalog.asset");
            var idx = EditorPrefs.GetInt("bot.world", 0);
            RunSettings.StartRun(worlds.worlds[Mathf.Clamp(idx, 0, worlds.worlds.Count - 1)]);

            var meta = AssetDatabase.LoadAssetAtPath<MetaCatalog>("Assets/_Project/ScriptableObjects/MetaCatalog.asset");
            var data = SaveSystem.Data;
            if (EditorPrefs.GetInt("bot.unlockAll", 0) == 1)
            {
                foreach (var c in meta.characters) if (!data.unlockedIds.Contains(c.unlock.id)) data.unlockedIds.Add(c.unlock.id);
                foreach (var w in meta.Weapons()) if (!data.unlockedIds.Contains(w.unlock.id)) data.unlockedIds.Add(w.unlock.id);
            }
            var ch = EditorPrefs.GetString("bot.char", "");
            if (ch.Length > 0)
                data.selectedCharacterId = ch;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Spawn()
        {
            if (EditorPrefs.GetInt("bot.enabled", 0) == 1 && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Game")
                new GameObject("BalanceBot").AddComponent<BalanceBot>();
        }

        private void Update()
        {
            var player = PlayerController.Instance;
            if (player == null || _done)
                return;

            if (Time.timeScale == 0f)
            {
                if (!PickCard())
                    Time.timeScale = 1f; // not a card window (nothing to pick): do not stay frozen
                return;
            }
            Time.timeScale = EditorPrefs.GetFloat("bot.timescale", 8f);

            var clock = RunClock.Instance;
            if (player.Health.IsDead)
            {
                Finish("DIED");
                return;
            }

            var bossAt = EditorPrefs.GetFloat("bot.bossAt", 0f);
            if (bossAt > 0f && !_bossCalled && clock.Elapsed >= bossAt)
            {
                _bossCalled = true;
                Object.FindAnyObjectByType<Altar>()?.DebugActivate();
                Debug.Log("BOT boss called at " + clock.Elapsed.ToString("0"));
            }
            if (_bossCalled && BossController.Current == null && clock.Elapsed > EditorPrefs.GetFloat("bot.bossAt", 0f) + 5f)
            {
                Finish("BOSS_DEAD");
                return;
            }

            if (clock.Elapsed >= _nextLog)
            {
                _nextLog += 60f;
                var inv = FindAnyObjectByType<Inventory>();
                Debug.Log("BOT t=" + (clock.Elapsed / 60f).ToString("0") + "m hp=" + player.Health.Current.ToString("0") + "/" + player.Health.Max.ToString("0")
                          + " lvl=" + player.Level.Level + " kills=" + (RunTracker.Instance != null ? RunTracker.Instance.Kills : 0)
                          + " alive=" + Enemy.Active.Count + " weapons=" + inv.WeaponCount + " passives=" + inv.PassiveCount
                          + " fps=" + (1f / Mathf.Max(0.0001f, Time.unscaledDeltaTime)).ToString("0"));
            }

            if (clock.Elapsed >= EditorPrefs.GetFloat("bot.endAt", 900f))
            {
                Finish("SURVIVED");
                return;
            }

            Steer(player);
        }

        private void Steer(PlayerController player)
        {
            Vector2 pos = player.transform.position;
            var flee = Vector2.zero;
            var nearestSqr = float.MaxValue;
            Vector2 nearestDir = Vector2.zero;
            var list = Enemy.Active;
            var boss = BossController.Current;
            for (var i = 0; i < list.Count; i++)
            {
                Vector2 d = pos - (Vector2)list[i].transform.position;
                var sqr = d.sqrMagnitude;
                if (sqr < nearestSqr) { nearestSqr = sqr; nearestDir = -d; }
                if (sqr < 3.5f && sqr > 0.0001f)
                    flee += d / sqr;
            }

            if (Time.time >= _nextGem)
            {
                _nextGem = Time.time + 0.3f;
                _gemDir = Vector2.zero;
                var best = 100f;
                foreach (var g in FindObjectsByType<XpGem>(FindObjectsSortMode.None))
                {
                    Vector2 to = (Vector2)g.transform.position - pos;
                    if (to.sqrMagnitude < best) { best = to.sqrMagnitude; _gemDir = to.normalized; }
                }
            }

            var dir = flee * 3f + _gemDir * 0.6f;
            if (nearestSqr > 6f && nearestSqr < 400f)
                dir += nearestDir.normalized * 0.8f;
            if (boss != null)
            {
                // kite the boss: keep ~5 units away, orbit
                Vector2 to = (Vector2)boss.transform.position - pos;
                var dist = to.magnitude;
                var tangent = new Vector2(-to.y, to.x).normalized;
                dir += (dist < 5f ? -to.normalized : dist > 7f ? to.normalized : Vector2.zero) + tangent * 0.5f;
            }
            MoveInput.Joystick = dir.sqrMagnitude > 0.0001f ? Vector2.ClampMagnitude(dir.normalized, 1f) : Vector2.zero;
        }

        private System.Collections.Generic.HashSet<string> _weaponNames;

        private bool PickCard()
        {
            var cards = FindObjectsByType<UpgradeCard>(FindObjectsSortMode.None);
            if (cards.Length == 0)
                return false;
            if (Time.unscaledTime < _nextPick)
                return true;
            _nextPick = Time.unscaledTime + 0.05f;

            if (_weaponNames == null)
            {
                _weaponNames = new System.Collections.Generic.HashSet<string>();
                var meta = AssetDatabase.LoadAssetAtPath<MetaCatalog>("Assets/_Project/ScriptableObjects/MetaCatalog.asset");
                foreach (var w in meta.Weapons()) _weaponNames.Add(Loc.T(w.displayName));
                foreach (var e in meta.pool.evolutions) if (e.result != null) _weaponNames.Add(Loc.T(e.result.displayName));
            }

            var titleField = typeof(UpgradeCard).GetField("title", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pick = cards[Random.Range(0, cards.Length)];
            foreach (var c in cards)
            {
                var t = ((Text)titleField.GetValue(c)).text;
                if (_weaponNames.Contains(t)) { pick = c; break; }
            }
            pick.GetComponentInChildren<Button>().onClick.Invoke();
            return true;
        }

        private void Finish(string reason)
        {
            var near = new System.Collections.Generic.Dictionary<string, int>();
            foreach (var en in Enemy.Active)
            {
                if (((Vector2)en.transform.position - (Vector2)PlayerController.Instance.transform.position).sqrMagnitude < 9f)
                {
                    var key = en.name.Replace("(Clone)", "");
                    near[key] = near.TryGetValue(key, out var c) ? c + 1 : 1;
                }
            }
            var nearText = "";
            foreach (var kv in near) nearText += kv.Key + "x" + kv.Value + " ";
            Debug.Log("BOT NEAR DEATH: " + nearText);
            _done = true;
            var clock = RunClock.Instance;
            var player = PlayerController.Instance;
            Debug.Log("BOT RESULT " + reason + " t=" + (clock.Elapsed / 60f).ToString("0.0") + "m lvl=" + player.Level.Level
                      + " kills=" + (RunTracker.Instance != null ? RunTracker.Instance.Kills : 0) + " zone=" + ZoneTimer.Instance.ZoneLevel
                      + " phase=" + ZoneTimer.Instance.Phase + " survived=" + ZoneTimer.Instance.SurvivedSeconds.ToString("0"));
            MoveInput.Joystick = Vector2.zero;
            EditorPrefs.SetString("bot.result", "w" + EditorPrefs.GetInt("bot.world", 0) + " " + (RunSettings.Character != null ? RunSettings.Character.name : "?") + " " + reason + " t=" + (clock.Elapsed / 60f).ToString("0.0") + "m lvl=" + player.Level.Level + " kills=" + (RunTracker.Instance != null ? RunTracker.Instance.Kills : 0) + " zone=" + ZoneTimer.Instance.ZoneLevel + " survived=" + ZoneTimer.Instance.SurvivedSeconds.ToString("0"));
        }
    }
}
#endif
