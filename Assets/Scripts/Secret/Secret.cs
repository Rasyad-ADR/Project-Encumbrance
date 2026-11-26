using Encumbrance.Resonance;
using UnityEngine;

namespace Encumbrance.Secret
{
    /// <summary>
    /// Up Up Down Down Left Right Left Right -> toggle immortal.
    /// </summary>
    public sealed class Secret : MonoBehaviour
    {
        [SerializeField] private ResonanceController resonance;

        private static readonly KeyCode[] Sequence =
        {
            KeyCode.UpArrow, KeyCode.UpArrow,
            KeyCode.DownArrow, KeyCode.DownArrow,
            KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.LeftArrow, KeyCode.RightArrow
        };

        private int index;

        private void Awake()
        {
            if (resonance == null) Debug.LogError("CheatCode: assign ResonanceController.", this);
        }

        private void Update()
        {
            if (resonance == null || !Input.anyKeyDown) return;

            if (Input.GetKeyDown(Sequence[index]))
            {
                index++;
                if (index >= Sequence.Length)
                {
                    index = 0;
                    resonance.Immortal = !resonance.Immortal;
                    Debug.Log("Cheat: immortal = " + resonance.Immortal);
                }
            }
            else
            {
                index = Input.GetKeyDown(Sequence[0]) ? 1 : 0;
            }
        }
    }
}