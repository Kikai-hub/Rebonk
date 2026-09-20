using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    [CreateAssetMenu(menuName = "Rebonk/World Catalog", fileName = "WorldCatalog")]
    public class WorldCatalog : ScriptableObject
    {
        public List<WorldConfig> worlds = new List<WorldConfig>();
    }
}
