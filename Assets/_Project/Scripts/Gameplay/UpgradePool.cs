using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>The set of upgrades that can appear on level-up, plus the evolution table. Add new content here, no code changes needed.</summary>
    [CreateAssetMenu(menuName = "Rebonk/Upgrade Pool", fileName = "UpgradePool")]
    public class UpgradePool : ScriptableObject
    {
        public List<UpgradeData> upgrades = new List<UpgradeData>();
        [Tooltip("Evolution results are NOT listed in 'upgrades': they can only be obtained through their recipe.")]
        public List<EvolutionData> evolutions = new List<EvolutionData>();
    }
}
