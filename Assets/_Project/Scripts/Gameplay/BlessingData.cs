using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>A temporary (whole-run) stat boost offered by a totem. Stackable: picking it twice doubles the bonus.</summary>
    [CreateAssetMenu(menuName = "Rebonk/Totem Blessing", fileName = "Blessing_")]
    public class BlessingData : UpgradeData
    {
        public StatType stat;
        [Tooltip("Bonus in the units of the stat (percent stats: 0.1 = +10%).")]
        public float value = 0.1f;

        public override int MaxLevel => 999;
        public override bool IsWeapon => false;

        public override string GetCardText(int level) => description;
    }
}
