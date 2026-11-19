using System;

namespace Encumbrance.Journal
{
    public static class TypingValidator
    {
        // Case-insensitive for letters; spaces and punctuation still require an exact match.
        public static bool Matches(char expected, char received)
        {
            return char.ToUpperInvariant(expected) == char.ToUpperInvariant(received);
        }
    }
}
