using UnityEngine;

namespace Encumbrance.Journal
{
    /// <summary>
    /// Optional prototype: scrolls an assigned background Transform on correct input.
    /// Keeps the player stationary. Use a test visual, not a shared scene object.
    /// </summary>
    public sealed class TypingParallaxPrototype : MonoBehaviour
    {
        [SerializeField] private TypingController controller;
        [SerializeField] private Transform backgroundToScroll;
        [SerializeField, Min(0f)] private float distancePerCharacter = 0.04f;
        [SerializeField] private Vector3 scrollDirection = Vector3.left;
        [SerializeField, Min(0.01f)] private float easeSpeed = 6f;

        private Vector3 targetPosition;
        private bool initialized;

        private void Awake()
        {
            if (backgroundToScroll != null)
            {
                targetPosition = backgroundToScroll.localPosition;
                initialized = true;
            }
        }

        private void OnEnable()
        {
            if (controller != null) controller.SentenceCompleted += HandleSentenceCompleted;
        }

        private void OnDisable()
        {
            if (controller != null) controller.SentenceCompleted -= HandleSentenceCompleted;
        }

        private void HandleSentenceCompleted()
        {
            if (backgroundToScroll == null) return;
            if (!initialized)
            {
                targetPosition = backgroundToScroll.localPosition;
                initialized = true;
            }
            if (scrollDirection.sqrMagnitude > 0.0001f)
                targetPosition += scrollDirection.normalized * distancePerCharacter;
        }

        private void Update()
        {
            if (!initialized || backgroundToScroll == null) return;
            float t = 1f - Mathf.Exp(-easeSpeed * Time.deltaTime);
            backgroundToScroll.localPosition = Vector3.Lerp(backgroundToScroll.localPosition, targetPosition, t);
        }
    }
}
