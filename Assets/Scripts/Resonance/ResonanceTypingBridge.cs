using Encumbrance.Journal;
using UnityEngine;

namespace Encumbrance.Resonance
{
    /// <summary>Translates typing events into Resonance state changes.</summary>
    public sealed class ResonanceTypingBridge : MonoBehaviour
    {
        [SerializeField] private TypingController typingController;
        [SerializeField] private ResonanceController resonance;
        [SerializeField, Min(0f)] private float baseMistakePenalty = 5f;
        [SerializeField, Min(0f)] private float prototypeWeightMultiplier = 1f;

        private void OnEnable()
        {
            if (typingController == null) return;
            typingController.CharacterMistyped += OnMistake;
            typingController.TypingStarted += OnTypingStarted;
            typingController.TypingBecameIdle += OnTypingIdle;
            typingController.ChapterCompleted += OnChapterCompleted;
        }

        private void OnDisable()
        {
            if (typingController == null) return;
            typingController.CharacterMistyped -= OnMistake;
            typingController.TypingStarted -= OnTypingStarted;
            typingController.TypingBecameIdle -= OnTypingIdle;
            typingController.ChapterCompleted -= OnChapterCompleted;
        }

        private void Start()
        {
            if (typingController == null || resonance == null)
                Debug.LogError("ResonanceTypingBridge: assign Typing Controller and Resonance references.", this);
        }

        private void OnMistake(char expected, char received)
        {
            if (resonance != null)
                resonance.ApplyLoss(baseMistakePenalty * prototypeWeightMultiplier);
        }

        private void OnTypingStarted()
        {
            if (resonance != null) resonance.SetTypingIdle(false);
        }

        private void OnTypingIdle()
        {
            if (resonance != null) resonance.SetTypingIdle(true);
        }

        private void OnChapterCompleted()
        {
            if (resonance != null) resonance.EndAscent();
        }
    }
}
