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
            if (journalText == null) return;
            if (string.IsNullOrEmpty(sentence)) { journalText.text = string.Empty; return; }
            typedCount = Mathf.Clamp(typedCount, 0, sentence.Length);
            var sb = new StringBuilder(sentence.Length + 80);
            sb.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(typedColor)).Append('>');
            sb.Append("<noparse>").Append(sentence.Substring(0, typedCount)).Append("</noparse>");
            sb.Append("</color><color=#").Append(ColorUtility.ToHtmlStringRGB(untypedColor)).Append('>');
            sb.Append("<noparse>").Append(sentence.Substring(typedCount)).Append("</noparse>");
            sb.Append("</color>");
            journalText.text = sb.ToString();
        }

    }
}
