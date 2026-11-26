using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Encumbrance.Journal
{
    // Uses Input System text events. Characters are queued, and each frame we send at most as many
    // characters as there were fresh key-downs, which suppresses OS keyboard auto-repeat
    // (auto-repeat emits text but not a new key-down) without dropping fast typing.
    public sealed class TypingInputReader : MonoBehaviour
    {
        [SerializeField] private TypingController controller;

        private readonly Queue<char> pendingCharacters = new Queue<char>();

        private void OnEnable()
        {
            if (Keyboard.current != null) Keyboard.current.onTextInput += OnTextInput;
        }

        private void OnDisable()
        {
            if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput;
            pendingCharacters.Clear();
        }

        private void OnTextInput(char character)
        {
            if (char.IsControl(character)) return;
            pendingCharacters.Enqueue(character);
        }

        private void LateUpdate()
        {
            if (pendingCharacters.Count == 0) return;

            int freshKeyDowns = 0;
            if (Keyboard.current != null)
            {
                foreach (KeyControl key in Keyboard.current.allKeys)
                    if (key.wasPressedThisFrame) freshKeyDowns++;
            }

            while (pendingCharacters.Count > 0)
            {
                char c = pendingCharacters.Dequeue();
                if (freshKeyDowns > 0 && controller != null)
                {
                    controller.ReceiveCharacter(c);
                    freshKeyDowns--;
                }
                // else: no matching key-down, so it's auto-repeat. Drop it.
            }
        }
    }
}