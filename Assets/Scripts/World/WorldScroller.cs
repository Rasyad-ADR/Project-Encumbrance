using Encumbrance.Journal;
using UnityEngine;

namespace Encumbrance.World
{
    /// <summary>
    /// Scrolls a content transform in its parent's local space for each correctly typed character.
    /// Put WorldContent under a rotated WorldRoot, and leave the player outside WorldRoot.
    /// </summary>
    public sealed class WorldScroller : MonoBehaviour
    {
        [SerializeField] private TypingController typingController;
        [SerializeField] private Transform worldContent;
        [SerializeField, Min(0f)] private float distancePerCharacter = 0.04f;
        [SerializeField] private Vector3 scrollDirection = Vector3.left;
        [SerializeField, Min(0.01f)] private float easeSpeed = 6f;

        private Vector3 targetLocalPosition;
        private bool scrollEnabled = true;
        private bool initialized;

        private void Awake()
        {
            if (typingController == null)
                Debug.LogError("WorldScroller: assign TypingController in the Inspector.", this);

            if (worldContent == null)
            {
                Debug.LogError("WorldScroller: assign WorldContent in the Inspector.", this);
                return;
            }

            targetLocalPosition = worldContent.localPosition;
            initialized = true;
        }

        private void OnEnable()
        {
            if (typingController != null)
                typingController.CharacterTyped += OnCharacterTyped;
        }

        private void OnDisable()
        {
            if (typingController != null)
                typingController.CharacterTyped -= OnCharacterTyped;
        }

        private void OnCharacterTyped(char character)
        {
            if (!scrollEnabled || !initialized || scrollDirection.sqrMagnitude < 0.0001f)
                return;

            targetLocalPosition += scrollDirection.normalized * distancePerCharacter;
        }

        private void Update()
        {
            if (!scrollEnabled || !initialized || worldContent == null)
                return;

            float blend = 1f - Mathf.Exp(-easeSpeed * Time.deltaTime);
            worldContent.localPosition = Vector3.Lerp(worldContent.localPosition, targetLocalPosition, blend);
        }

        public void SetScrollEnabled(bool enabled)
        {
            if (!enabled && worldContent != null)
                targetLocalPosition = worldContent.localPosition;

            scrollEnabled = enabled;
        }
    }
}
