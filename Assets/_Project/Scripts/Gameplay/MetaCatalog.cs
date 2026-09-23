using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Everything that can be unlocked: characters and the weapons in the upgrade pool.</summary>
    [CreateAssetMenu(menuName = "Rebonk/Meta Catalog", fileName = "MetaCatalog")]
    public class MetaCatalog : ScriptableObject
    {
        public List<CharacterStats> characters = new List<CharacterStats>();
        public UpgradePool pool;

        public CharacterStats DefaultCharacter
        {
            get
            {
                foreach (var c in characters)
                    if (c.unlock.unlockedByDefault)
                        return c;
                return characters.Count > 0 ? characters[0] : null;
            }
        }

        public CharacterStats Find(string id)
        {
            foreach (var c in characters)
                if (c.unlock.id == id)
                    return c;
            return null;
        }

        /// <summary>The selected character from the save, or the default one if it is unknown or still locked.</summary>
        public CharacterStats SelectedCharacter()
        {
            var selected = Find(SaveSystem.Data.selectedCharacterId);
            return selected != null && selected.unlock.IsUnlocked ? selected : DefaultCharacter;
        }

        public IEnumerable<WeaponData> Weapons()
        {
            if (pool == null)
                yield break;
            foreach (var u in pool.upgrades)
                if (u is WeaponData w)
                    yield return w;
        }

        public IEnumerable<PassiveData> Passives()
        {
            if (pool == null)
                yield break;
            foreach (var u in pool.upgrades)
                if (u is PassiveData p)
                    yield return p;
        }

        /// <summary>Unlocks everything whose condition is now met. Returns display names of the new unlocks.</summary>
        public List<string> EvaluateUnlocks(SaveData data)
        {
            var result = new List<string>();

            foreach (var c in characters)
            {
                if (!c.unlock.IsUnlocked && c.unlock.IsMet(data))
                {
                    data.unlockedIds.Add(c.unlock.id);
                    result.Add(c.displayName);
                }
            }

            foreach (var w in Weapons())
            {
                if (!w.unlock.IsUnlocked && w.unlock.IsMet(data))
                {
                    data.unlockedIds.Add(w.unlock.id);
                    result.Add(w.displayName);
                }
            }

            return result;
        }
    }
}
