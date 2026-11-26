using System.Text;
using TMPro;
using UnityEngine;

namespace Encumbrance.Journal
{
    public sealed class JournalView : MonoBehaviour
    {
        [SerializeField] private TypingController controller;
        [SerializeField] private TMP_Text journalText;
        [SerializeField] private Color32 typedColor = new Color32(32, 32, 32, 255);
        [SerializeField] private Color32 untypedColor = new Color32(146, 146, 146, 255);
        [SerializeField, Min(0.1f)] private float blinkInterval = 0.5f;

        private string currentSentence = string.Empty;
        private int currentTypedCount;
        private float blinkStart;
        private bool caretVisible = true;

        private void OnEnable()
        {
            if (controller != null) controller.SentenceChanged += Render;
            if (controller != null) Render(controller.CurrentSentence, controller.CurrentCharacterIndex);
        }

        private void OnDisable()
        {
            if (controller != null) controller.SentenceChanged -= Render;
        }

        private void Render(string sentence, int typedCount)
        {
            currentSentence = sentence;
            currentTypedCount = typedCount;
            blinkStart = Time.unscaledTime;   // caret stays solid right after a keypress
            caretVisible = true;
            Refresh();
        }

        private void Update()
        {
            bool visible = (int)((Time.unscaledTime - blinkStart) / blinkInterval) % 2 == 0;
            if (visible == caretVisible) return;
            caretVisible = visible;
            Refresh();
        }

        private void Refresh()
        {
            if (journalText == null) return;
            if (string.IsNullOrEmpty(currentSentence)) { journalText.text = string.Empty; return; }

            int count = Mathf.Clamp(currentTypedCount, 0, currentSentence.Length);
            string caretAlpha = caretVisible ? "FF" : "00";

            var sb = new StringBuilder(currentSentence.Length + 120);
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(typedColor)).Append('>');
            sb.Append("<noparse>").Append(currentSentence.Substring(0, count)).Append("</noparse>");
            sb.Append("</color>");
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(typedColor)).Append(caretAlpha).Append(">|</color>");
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(untypedColor)).Append('>');
            sb.Append("<noparse>").Append(currentSentence.Substring(count)).Append("</noparse>");
            sb.Append("</color>");
            journalText.text = sb.ToString();
        }
    }
}