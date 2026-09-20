using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>
    /// One row of the evolution table: a weapon at MAX level plus a passive item in the inventory
    /// unlocks an evolution card on the next level-up. Taking it replaces the base weapon with the result.
    /// Pure data: add rows in the UpgradePool asset, no code changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Rebonk/Evolution", fileName = "Evolution_")]
    public class EvolutionData : ScriptableObject
    {
        public WeaponData baseWeapon;
        public PassiveData requiredPassive;
        [Min(1)] public int requiredPassiveLevel = 1;
        public WeaponData result;
    }
}
