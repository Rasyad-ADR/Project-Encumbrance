using UnityEngine;

namespace Encumbrance.Player
{
    public sealed class PlayerWalkVisual : MonoBehaviour
    {
        [SerializeField] private Transform worldContent;
        [SerializeField] private SpriteRenderer characterSprite;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;
        [SerializeField, Min(0f)] private float movementThreshold = 0.025f;
        [SerializeField, Min(0)] private int idleFrameIndex = 0;

        private Vector3 previousPosition;
        private float frameClock;
        private int frameIndex;
        private bool ready;

        private void OnEnable()
        {
            ready = worldContent != null && characterSprite != null &&
                    walkFrames != null && walkFrames.Length > 0;
            if (ready)
                foreach (Sprite frame in walkFrames)
                    if (frame == null) { ready = false; break; }

            if (!ready)
            {
                Debug.LogError("PlayerWalkVisual: assign WorldContent, CharacterSprite and all walk frames.", this);
                return;
            }
            previousPosition = worldContent.localPosition;
            SetIdle();
        }

        private void LateUpdate()
        {
            if (!ready || worldContent == null || characterSprite == null) return;
            Vector3 position = worldContent.localPosition;
            float distance = Vector3.Distance(position, previousPosition);
            previousPosition = position;
            float dt = Time.deltaTime;
            if (dt <= 0f || distance <= movementThreshold * dt)
            {
                SetIdle();
                return;
            }

            frameClock += dt * Mathf.Max(1f, framesPerSecond);
            int steps = Mathf.FloorToInt(frameClock);
            if (steps > 0)
            {
                frameClock -= steps;
                frameIndex = (frameIndex + steps) % walkFrames.Length;
            }
            characterSprite.sprite = walkFrames[frameIndex];
        }

        private void SetIdle()
        {
            frameClock = 0f;
            frameIndex = Mathf.Clamp(idleFrameIndex, 0, walkFrames.Length - 1);
            characterSprite.sprite = walkFrames[frameIndex];
        }
    }
}
