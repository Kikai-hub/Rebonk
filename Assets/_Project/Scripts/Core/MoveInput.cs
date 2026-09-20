using UnityEngine;

namespace Rebonk.Core
{
    /// <summary>
    /// Device-agnostic movement input. Sources (virtual joystick, keyboard, ...) write their
    /// own channel; gameplay only reads <see cref="Value"/>. Keeps the PC port a matter of adding a source.
    /// </summary>
    public static class MoveInput
    {
        public static Vector2 Joystick;
        public static Vector2 Keyboard;

        public static Vector2 Value
        {
            get
            {
                var v = Joystick.sqrMagnitude > 0.0001f ? Joystick : Keyboard;
                return Vector2.ClampMagnitude(v, 1f);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState()
        {
            Joystick = default;
            Keyboard = default;
        }
    }
}
