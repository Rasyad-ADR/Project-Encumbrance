using UnityEngine;

namespace Encumbrance.Journal
{
    /// <summary>
    /// Debug-only listener; does not decrease Resonance or change scene flow.
    /// Remove/disable after connecting actual systems.
    /// </summary>
    public sealed class TypingEventDebug : MonoBehaviour
    {
        [SerializeField] private TypingController controller;
        [SerializeField] private bool logCorrectCharacters = false;

        private void OnEnable()
        {
            if (controller == null) return;
            controller.CharacterTyped += OnCorrect;
            controller.CharacterMistyped += OnMistake;
            controller.TypingBecameIdle += OnIdle;
            controller.ChapterCompleted += OnComplete;
        }

        private void OnDisable()
        {
            if (controller == null) return;
            controller.CharacterTyped -= OnCorrect;
            controller.CharacterMistyped -= OnMistake;
            controller.TypingBecameIdle -= OnIdle;
            controller.ChapterCompleted -= OnComplete;
        }

        private void OnCorrect(char character)
        {
            if (logCorrectCharacters) Debug.Log($"Journal: correct input '{character}'", this);
        }

        private void OnMistake(char expected, char received)
        {
            Debug.Log($"Journal: mistake — expected '{expected}', received '{received}'. Resonance hook ready.", this);
        }

        private void OnIdle() => Debug.Log("Journal: idle", this);
        private void OnComplete() => Debug.Log("Journal: chapter completed", this);
    }
}
