using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class JournalTypingSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text journalText;
    [SerializeField] private TMP_Text pageNumberText;

    [Header("Journal Content")]
    [TextArea(3, 10)]
    [SerializeField] private string[] pages;

    [Header("Word Colors")]
    [SerializeField]
    private Color writtenColor =
        new Color(0.1f, 0.1f, 0.1f);

    [SerializeField]
    private Color unwrittenColor =
        new Color(0.55f, 0.55f, 0.55f);

    [Header("Page Transition")]
    [SerializeField] private float pageAdvanceDelay = 0.7f;

    [Header("Events")]
    public UnityEvent onWrongInput;
    public UnityEvent onPageChanged;
    public UnityEvent onJournalCompleted;

    private int pageIndex;
    private int charIndex;

    private bool isAdvancing;
    private bool isComplete;

    private Keyboard activeKeyboard;

    private string CurrentPage
    {
        get
        {
            if (pages == null || pageIndex >= pages.Length)
                return string.Empty;

            return pages[pageIndex] ?? string.Empty;
        }
    }

    private void OnEnable()
    {
        ConnectKeyboard();
    }

    private void OnDisable()
    {
        DisconnectKeyboard();
    }

    private void Update()
    {
        // Reconnect if the active keyboard changes.
        if (activeKeyboard != Keyboard.current)
        {
            DisconnectKeyboard();
            ConnectKeyboard();
        }
    }

    private void Start()
    {
        if (journalText == null)
        {
            Debug.LogError(
                "JournalTypingSystem: Journal Text is not assigned."
            );
            enabled = false;
            return;
        }

        if (pages == null || pages.Length == 0)
        {
            Debug.LogError(
                "JournalTypingSystem: Add at least one journal page."
            );
            enabled = false;
            return;
        }

        ShowCurrentPage();
    }

    private void ConnectKeyboard()
    {
        activeKeyboard = Keyboard.current;

        if (activeKeyboard != null)
            activeKeyboard.onTextInput += HandleTextInput;
    }

    private void DisconnectKeyboard()
    {
        if (activeKeyboard != null)
            activeKeyboard.onTextInput -= HandleTextInput;

        activeKeyboard = null;
    }

    private void HandleTextInput(char typedChar)
    {
        if (isAdvancing || isComplete)
            return;

        string text = CurrentPage;

        // Newlines are visual formatting, not characters
        // the player needs to type.
        SkipLineBreaks();

        if (charIndex >= text.Length)
            return;

        char expectedChar = text[charIndex];

        // Ignore letter case, but preserve punctuation
        // and spaces.
        bool isCorrect =
            char.ToLowerInvariant(typedChar) ==
            char.ToLowerInvariant(expectedChar);

        if (isCorrect)
        {
            charIndex++;

            SkipLineBreaks();
            RenderPage();

            if (charIndex >= text.Length)
                StartCoroutine(AdvancePage());
        }
        else
        {
            // Connect this event to the Resonance system.
            if (onWrongInput != null)
                onWrongInput.Invoke();
        }
    }

    private void SkipLineBreaks()
    {
        string text = CurrentPage;

        while (charIndex < text.Length &&
              (text[charIndex] == '\n' ||
               text[charIndex] == '\r'))
        {
            charIndex++;
        }
    }

    private void ShowCurrentPage()
    {
        charIndex = 0;
        SkipLineBreaks();

        RenderPage();

        if (pageNumberText != null)
        {
            pageNumberText.text =
                $"Page {pageIndex + 1} / {pages.Length}";
        }
    }

    private void RenderPage()
    {
        string text = CurrentPage;
        StringBuilder output = new StringBuilder();

        int i = 0;

        while (i < text.Length)
        {
            // Keep spaces and line breaks so that
            // TextMeshPro can lay out the passage.
            if (char.IsWhiteSpace(text[i]))
            {
                output.Append(text[i]);
                i++;
                continue;
            }

            // Find the next complete word, including
            // any punctuation attached to it.
            int wordStart = i;

            while (i < text.Length &&
                   !char.IsWhiteSpace(text[i]))
            {
                i++;
            }

            string word = text.Substring(
                wordStart, i - wordStart
            );

            // A word becomes black only after every
            // character in it has been typed.
            bool isWritten = charIndex >= i;

            Color wordColor =
                isWritten ? writtenColor : unwrittenColor;

            output.Append("<color=#");
            output.Append(
                ColorUtility.ToHtmlStringRGB(wordColor)
            );
            output.Append(">");
            output.Append(EscapeRichText(word));
            output.Append("</color>");
        }

        journalText.color = unwrittenColor;
        journalText.text = output.ToString();
    }

    private static string EscapeRichText(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private IEnumerator AdvancePage()
    {
        isAdvancing = true;

        yield return new WaitForSeconds(pageAdvanceDelay);

        if (pageIndex + 1 < pages.Length)
        {
            pageIndex++;
            ShowCurrentPage();

            if (onPageChanged != null)
                onPageChanged.Invoke();
        }
        else
        {
            // The final page has been completed.
            isComplete = true;

            if (onJournalCompleted != null)
                onJournalCompleted.Invoke();
        }

        isAdvancing = false;
    }
}