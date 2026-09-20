using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>The blessings a totem can offer. Add new ones here, no code needed.</summary>
    [CreateAssetMenu(menuName = "Rebonk/Blessing Pool", fileName = "BlessingPool")]
    public class BlessingPool : ScriptableObject
    {
        public List<BlessingData> blessings = new List<BlessingData>();

        /// <summary>Fills <paramref name="result"/> with up to <paramref name="count"/> distinct random blessings.</summary>
        public void Roll(int count, List<UpgradeChoice> result)
        {
            result.Clear();
            var pool = new List<BlessingData>(blessings);
            while (result.Count < count && pool.Count > 0)
            {
                var i = Random.Range(0, pool.Count);
                result.Add(new UpgradeChoice { Data = pool[i], NextLevel = 1 });
                pool.RemoveAt(i);
            }
        }
    }
}
