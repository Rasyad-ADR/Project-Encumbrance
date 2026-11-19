using System.Collections.Generic;
using UnityEngine;

namespace Encumbrance.Journal
{
    [CreateAssetMenu(fileName = "JournalChapter", menuName = "Encumbrance/Journal Chapter")]
    public sealed class JournalChapterData : ScriptableObject
    {
        [TextArea(2, 5)] public List<string> sentences = new List<string>();
    }
}
