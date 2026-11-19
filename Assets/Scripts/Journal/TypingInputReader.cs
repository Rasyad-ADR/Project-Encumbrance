using UnityEngine;
using UnityEngine.InputSystem;

namespace Encumbrance.Journal
{
    // Uses Input System text events. A key-down in the same frame gates text input,
    // suppressing OS keyboard auto-repeat (which emits text but not a fresh key-down).
    public sealed class TypingInputReader : MonoBehaviour
    {
        [SerializeField] private TypingController controller;
        private bool hasPendingCharacter;
        private char pendingCharacter;

        private void OnEnable()
        {
            if (Keyboard.current != null) Keyboard.current.onTextInput += OnTextInput;
        }

        private void OnDisable()
        {
            if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput;
            hasPendingCharacter = false;
        }

        private void OnTextInput(char character)
        {
            if (char.IsControl(character)) return;
            // One typed character per key-down/frame for the keyboard prototype.
            if (!hasPendingCharacter)
            {
                pendingCharacter = character;
                hasPendingCharacter = true;
            }
        }

        private void LateUpdate()
        {
            if (!hasPendingCharacter) return;
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame && controller != null)
                controller.ReceiveCharacter(pendingCharacter);
            hasPendingCharacter = false;
        }
    }
}
