using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rebonk.Gameplay
{
    public struct UpgradeChoice
    {
        /// <summary>What the card shows/gives. For evolutions this is the evolved weapon.</summary>
        public UpgradeData Data;
        public int NextLevel;
        /// <summary>Set when the card is an evolution; taking it replaces the base weapon.</summary>
        public EvolutionData Evolution;

        public bool IsEvolution => Evolution != null;
    }

    /// <summary>Owned weapons and passives with their levels, slot limits, evolutions and level-up choice generation.</summary>
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private UpgradePool pool;
        [SerializeField] private int maxWeapons = 6;
        [SerializeField] private int maxPassives = 6;

        private readonly Dictionary<UpgradeData, int> _levels = new Dictionary<UpgradeData, int>();
        private readonly Dictionary<PassiveData, int> _passives = new Dictionary<PassiveData, int>();
        private readonly Dictionary<WeaponData, Weapon> _weapons = new Dictionary<WeaponData, Weapon>();
        private readonly List<UpgradeChoice> _weaponCandidates = new List<UpgradeChoice>();
        private readonly List<UpgradeChoice> _itemCandidates = new List<UpgradeChoice>();
        private readonly List<UpgradeChoice> _endlessCandidates = new List<UpgradeChoice>();
        private PlayerController _player;

        /// <summary>Raised whenever any inventory finishes an evolution (run statistics).</summary>
        public static event Action WeaponEvolved;

        public int WeaponCount => _weapons.Count;
        public int PassiveCount => _passives.Count;
        public bool Owns(WeaponData weapon) => _weapons.ContainsKey(weapon);
        public event Action Changed;

        private void Start()
        {
            _player = PlayerController.Instance;
            var character = _player.Base;

            if (character.randomStartWeapon)
            {
                var random = PickRandomUnlockedWeapon();
                if (random != null)
                    Apply(random);
            }
            else if (character.startingWeapon != null)
            {
                Apply(character.startingWeapon);
            }

            foreach (var extra in character.extraStartingWeapons)
                if (extra != null && GetLevel(extra) == 0)
                    Apply(extra);
        }

        private WeaponData PickRandomUnlockedWeapon()
        {
            var options = new List<WeaponData>();
            foreach (var u in pool.upgrades)
                if (u is WeaponData w && w.unlock.IsUnlocked)
                    options.Add(w);
            return options.Count > 0 ? options[UnityEngine.Random.Range(0, options.Count)] : null;
        }

        public int GetLevel(UpgradeData data) => _levels.TryGetValue(data, out var lvl) ? lvl : 0;

        public void Apply(UpgradeChoice choice)
        {
            if (choice.IsEvolution)
                Evolve(choice.Evolution);
            else
                Apply(choice.Data);
        }

        public void Apply(UpgradeData data)
        {
            // Totem blessings are run-long stat boosts, not slot items.
            if (data is BlessingData blessing)
            {
                _player.Stats.AddBlessing(blessing);
                Changed?.Invoke();
                return;
            }

            var next = GetLevel(data) + 1;
            if (next > data.MaxLevel && !(next > 1 && data.AllowsEndless))
                return;

            _levels[data] = next;

            if (data is WeaponData weaponData)
            {
                if (_weapons.TryGetValue(weaponData, out var weapon))
                    weapon.SetLevel(next);
                else
                    SpawnWeapon(weaponData, next);
            }
            else if (data is PassiveData passive)
            {
                _passives[passive] = next;
                _player.Stats.Recalculate(_passives);
            }

            Changed?.Invoke();
        }

        private void Evolve(EvolutionData evolution)
        {
            if (_weapons.TryGetValue(evolution.baseWeapon, out var oldWeapon))
            {
                _weapons.Remove(evolution.baseWeapon);
                Destroy(oldWeapon.gameObject);
            }

            var result = evolution.result;
            _levels[result] = result.MaxLevel;
            SpawnWeapon(result, result.MaxLevel);
            Changed?.Invoke();
            WeaponEvolved?.Invoke();
        }

        private void SpawnWeapon(WeaponData data, int level)
        {
            var weapon = Instantiate(data.weaponPrefab, _player.transform);
            weapon.Init(data, _player);
            weapon.SetLevel(level);
            _weapons[data] = weapon;
        }

        /// <summary>
        /// Fills <paramref name="result"/> with up to <paramref name="count"/> distinct choices.
        /// Available evolutions always come first; the rest are random valid upgrades.
        /// </summary>
        public void GetChoices(int count, List<UpgradeChoice> result)
        {
            result.Clear();

            // 1) evolutions: base weapon at max level + required passive owned
            foreach (var evo in pool.evolutions)
            {
                if (result.Count >= count)
                    break;
                if (evo.baseWeapon == null || evo.result == null || evo.requiredPassive == null)
                    continue;
                if (!_weapons.ContainsKey(evo.baseWeapon) || GetLevel(evo.baseWeapon) < evo.baseWeapon.MaxLevel)
                    continue;
                if (GetLevel(evo.requiredPassive) < evo.requiredPassiveLevel)
                    continue;

                result.Add(new UpgradeChoice { Data = evo.result, NextLevel = 1, Evolution = evo });
            }

            // 2) regular upgrades, split into weapons and items
            _weaponCandidates.Clear();
            _itemCandidates.Clear();
            _endlessCandidates.Clear();
            foreach (var data in pool.upgrades)
            {
                var level = GetLevel(data);
                if (level >= data.MaxLevel)
                {
                    // Owned and maxed: keeps growing forever, but only fills cards nothing else can.
                    if (data.AllowsEndless && IsOwned(data))
                        _endlessCandidates.Add(new UpgradeChoice { Data = data, NextLevel = level + 1 });
                    continue;
                }
                if (level == 0 && !HasFreeSlot(data))
                    continue;
                // Weapons still locked in the meta progression cannot be offered as new cards.
                if (level == 0 && data is WeaponData locked && !locked.unlock.IsUnlocked)
                    continue;

                var choice = new UpgradeChoice { Data = data, NextLevel = level + 1 };
                if (data.IsWeapon)
                    _weaponCandidates.Add(choice);
                else
                    _itemCandidates.Add(choice);
            }

            // Each free card is a coin flip between "weapon" and "item" so the big item list
            // does not drown out weapons (and vice versa); if one side is empty the other fills in.
            while (result.Count < count && (_weaponCandidates.Count > 0 || _itemCandidates.Count > 0))
            {
                var fromWeapons = UnityEngine.Random.value < 0.5f;
                if (fromWeapons && _weaponCandidates.Count == 0) fromWeapons = false;
                if (!fromWeapons && _itemCandidates.Count == 0) fromWeapons = true;

                var list = fromWeapons ? _weaponCandidates : _itemCandidates;
                var index = UnityEngine.Random.Range(0, list.Count);
                result.Add(list[index]);
                list.RemoveAt(index);
            }

            // Evolved weapons are not in the pool but are owned and can grow endlessly too.
            foreach (var kv in _weapons)
            {
                var weapon = kv.Key;
                if (!pool.upgrades.Contains(weapon) && weapon.AllowsEndless)
                    _endlessCandidates.Add(new UpgradeChoice { Data = weapon, NextLevel = GetLevel(weapon) + 1 });
            }

            // Everything at its designed maximum: endless levels fill the remaining cards.
            while (result.Count < count && _endlessCandidates.Count > 0)
            {
                var index = UnityEngine.Random.Range(0, _endlessCandidates.Count);
                result.Add(_endlessCandidates[index]);
                _endlessCandidates.RemoveAt(index);
            }
        }

        private bool IsOwned(UpgradeData data) =>
            data is WeaponData w ? _weapons.ContainsKey(w) : data is PassiveData p && _passives.ContainsKey(p);

        private bool HasFreeSlot(UpgradeData data) =>
            data.IsWeapon ? _weapons.Count < maxWeapons : _passives.Count < maxPassives;
    }
}
