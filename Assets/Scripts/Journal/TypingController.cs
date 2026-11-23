using System;
using UnityEngine;

namespace Encumbrance.Journal
{
    public sealed class TypingController : MonoBehaviour
    {
        [SerializeField] private JournalChapterData chapter;
        [SerializeField, Min(0.01f)] private float idleThresholdSeconds = 0.15f;

        public event Action<char> CharacterTyped;
        public event Action<char, char> CharacterMistyped;
        public event Action SentenceCompleted;
        public event Action ChapterCompleted;
        public event Action TypingStarted;
        public event Action TypingBecameIdle;
        public event Action<string, int> SentenceChanged;

        public string CurrentSentence { get; private set; } = string.Empty;
        public int CurrentCharacterIndex { get; private set; }
        public int CurrentSentenceIndex { get; private set; }
        public bool IsChapterCompleted { get; private set; }
        public bool IsReady { get; private set; }

        private bool inputEnabled = true;
        private bool isActiveTyping;
        private float lastInputTime;

        private void Start() => BeginChapter();

        public void BeginChapter()
        {
            inputEnabled = true;
            IsReady = false;
            IsChapterCompleted = false;
            isActiveTyping = false;
            CurrentCharacterIndex = 0;
            CurrentSentenceIndex = 0;
            CurrentSentence = string.Empty;

            if (chapter == null || chapter.sentences == null || chapter.sentences.Count == 0)
            {
                Debug.LogError("Journal: assign a non-empty JournalChapterData in the Inspector.", this);
                return;
            }
            for (int i = 0; i < chapter.sentences.Count; i++)
            {
                if (string.IsNullOrEmpty(chapter.sentences[i]))
                {
                    Debug.LogError($"Journal: sentence {i} is null/empty. Fix the chapter data.", this);
                    return;
                }
            }
            IsReady = true;
            LoadSentence();
        }

        public void SetInputEnabled(bool enabled)
        {
            if (inputEnabled == enabled) return;

            inputEnabled = enabled;
            if (!enabled && isActiveTyping)
            {
                isActiveTyping = false;
                TypingBecameIdle?.Invoke();
            }
        }

        public void ReceiveCharacter(char input)
        {
            if (!inputEnabled || !IsReady || IsChapterCompleted || char.IsControl(input)) return;
            if (!isActiveTyping)
            {
                isActiveTyping = true;
                TypingStarted?.Invoke();
            }
            lastInputTime = Time.unscaledTime;

            char expected = CurrentSentence[CurrentCharacterIndex];
            if (!TypingValidator.Matches(expected, input))
            {
                CharacterMistyped?.Invoke(expected, input);
                return;
            }

            CurrentCharacterIndex++;
            CharacterTyped?.Invoke(input);
            SentenceChanged?.Invoke(CurrentSentence, CurrentCharacterIndex);

            if (CurrentCharacterIndex < CurrentSentence.Length) return;

            SentenceCompleted?.Invoke();
            CurrentSentenceIndex++;
            if (CurrentSentenceIndex >= chapter.sentences.Count)
            {
                IsChapterCompleted = true;
                IsReady = false;
                if (isActiveTyping)
                {
                    isActiveTyping = false;
                    TypingBecameIdle?.Invoke();
                }
                ChapterCompleted?.Invoke();
                return;
            }
            LoadSentence(); // Immediate transition; this key never counts for the next sentence.
        }

        private void LoadSentence()
        {
            CurrentSentence = chapter.sentences[CurrentSentenceIndex];
            CurrentCharacterIndex = 0;
            SentenceChanged?.Invoke(CurrentSentence, 0);
        }

        private void Update()
        {
            if (inputEnabled && IsReady && isActiveTyping && Time.unscaledTime - lastInputTime >= idleThresholdSeconds)
            {
                isActiveTyping = false;
                TypingBecameIdle?.Invoke();
            }
        }
    }
}
