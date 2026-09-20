using UnityEngine;

namespace Rebonk.Gameplay
{
    /// <summary>Choices the game scene reads: which world, which character, and the map seed.</summary>
    public static class RunSettings
    {
        public static WorldConfig World { get; private set; }
        public static CharacterStats Character { get; private set; }
        public static int Seed { get; private set; }

        public static void StartRun(WorldConfig world)
        {
            World = world;
            NewSeed();
        }

        /// <summary>Set by the game scene at load from the saved selection.</summary>
        public static void SetCharacter(CharacterStats character) => Character = character;

        public static void NewSeed() => Seed = Random.Range(1, int.MaxValue / 2);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            World = null;
            Character = null;
            Seed = 0;
        }
    }
}
