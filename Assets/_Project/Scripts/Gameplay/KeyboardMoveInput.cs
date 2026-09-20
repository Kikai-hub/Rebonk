using Rebonk.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rebonk.Gameplay
{
    /// <summary>WASD / arrows source for editor testing and the future PC port.</summary>
    public class KeyboardMoveInput : MonoBehaviour
    {
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return;

            var x = (kb.dKey.isPressed || kb.rightArrowKey.isPressed ? 1f : 0f)
                    - (kb.aKey.isPressed || kb.leftArrowKey.isPressed ? 1f : 0f);
            var y = (kb.wKey.isPressed || kb.upArrowKey.isPressed ? 1f : 0f)
                    - (kb.sKey.isPressed || kb.downArrowKey.isPressed ? 1f : 0f);

            MoveInput.Keyboard = new Vector2(x, y).normalized;
        }

        private void OnDisable() => MoveInput.Keyboard = default;
    }
}
