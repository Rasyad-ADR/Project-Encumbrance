using System;
using System.Collections;
using Encumbrance.Journal;
using Encumbrance.Resonance;
using Encumbrance.World;
using TMPro;
using UnityEngine;

namespace Encumbrance.Narration
{
    [Serializable]
    public sealed class NarrationEntry
    {
        [Min(1)] public int afterSentences = 5;          // muncul setelah sentence ke-berapa
        [TextArea(2, 5)] public string text = "...";     // isi narasi, bisa diubah di Inspector
        [Min(0f)] public float holdSeconds = 2.5f;       // lama teks tampil penuh
    }

    public sealed class Narration_System : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TypingController typing;
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private Terrain_Moving terrainMovement;
        [SerializeField] private Mountain_Moving mountainMovement;
        [SerializeField] private WorldScroller worldScroller;

        [Header("UI")]
        [SerializeField] private CanvasGroup narrationGroup;
        [SerializeField] private TMP_Text narrationText;
        [SerializeField, Min(0f)] private float fadeInSeconds = 1f;
        [SerializeField, Min(0f)] private float fadeOutSeconds = 1f;

        [Header("Narrations (bisa ditambah/diubah)")]
        [SerializeField] private NarrationEntry[] narrations;

        private int completedSentences;
        private bool playing;
        private bool[] played;

        private void Awake()
        {
            if (typing == null) Debug.LogError("Narration_System: assign TypingController.", this);
            if (resonance == null) Debug.LogError("Narration_System: assign ResonanceController.", this);
            if (narrationGroup == null || narrationText == null)
                Debug.LogError("Narration_System: assign Narration Group and Narration Text.", this);

            played = new bool[narrations != null ? narrations.Length : 0];

            if (narrationGroup != null)
            {
                narrationGroup.alpha = 0f;
                narrationGroup.blocksRaycasts = false;
                narrationGroup.interactable = false;
                narrationGroup.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (typing != null) typing.SentenceCompleted += OnSentenceCompleted;
        }

        private void OnDisable()
        {
            if (typing != null) typing.SentenceCompleted -= OnSentenceCompleted;
        }

        private void OnSentenceCompleted()
        {
            completedSentences++;
            if (playing || typing == null || narrations == null) return;
            if (completedSentences >= typing.TotalSentenceCount) return; // chapter selesai

            for (int i = 0; i < narrations.Length; i++)
            {
                if (played[i] || completedSentences != narrations[i].afterSentences) continue;
                played[i] = true;
                StartCoroutine(PlayNarration(narrations[i]));
                break;
            }
        }

        private IEnumerator PlayNarration(NarrationEntry entry)
        {
            playing = true;

            // Pause
            typing.SetInputEnabled(false);
            resonance.EndAscent();
            SetMovement(false);

            narrationText.text = entry.text;
            narrationGroup.alpha = 0f;
            narrationGroup.gameObject.SetActive(true);

            yield return Fade(0f, 1f, fadeInSeconds);
            yield return new WaitForSecondsRealtime(entry.holdSeconds);
            yield return Fade(1f, 0f, fadeOutSeconds);

            narrationGroup.gameObject.SetActive(false);

            // Lanjut
            if (resonance.Current > 0f) resonance.StartAscent();
            SetMovement(true);
            typing.SetInputEnabled(true);
            playing = false;
        }

        private IEnumerator Fade(float from, float to, float seconds)
        {
            float t = 0f;
            narrationGroup.alpha = from;
            while (t < seconds)
            {
                t += Time.unscaledDeltaTime;
                narrationGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
                yield return null;
            }
            narrationGroup.alpha = to;
        }

        // Ganti teks narasi dari script lain. Index dimulai dari 0.
        public void SetNarrationText(int index, string newText)
        {
            if (narrations == null || index < 0 || index >= narrations.Length) return;
            narrations[index].text = newText;
        }

        private void SetMovement(bool value)
        {
            if (terrainMovement != null) terrainMovement.SetMovementEnabled(value);
            else if (worldScroller != null) worldScroller.SetScrollEnabled(value);
            if (mountainMovement != null) mountainMovement.SetMovementEnabled(value);
        }
    }
}