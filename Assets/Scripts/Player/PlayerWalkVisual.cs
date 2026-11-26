using UnityEngine;

namespace Encumbrance.Player
{
    public sealed class PlayerWalkVisual : MonoBehaviour
    {
        [SerializeField] private Transform worldContent;
        [SerializeField] private Terrain_Moving terrainMovement;
        private Transform MotionTarget => terrainMovement != null ? terrainMovement.MovementTransform : worldContent;
        [SerializeField] private SpriteRenderer characterSprite;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField, Min(1f)] private float framesPerSecond = 12f;
        [SerializeField, Min(0f)] private float movementThreshold = 0.025f;
        [SerializeField, Min(0)] private int idleFrameIndex = 0;
        [SerializeField, Min(0f)] private float movementGracePeriod = 0.15f;
        private float graceRemaining;

        private Vector3 previousPosition;
        private float frameClock;
        private int frameIndex;
        private bool ready;

        private void OnEnable()
        {
            ready = MotionTarget != null && characterSprite != null &&
                    walkFrames != null && walkFrames.Length > 0;
            if (ready)
                foreach (Sprite frame in walkFrames)
                    if (frame == null) { ready = false; break; }

            if (!ready)
            {
                Debug.LogError("PlayerWalkVisual: assign WorldContent, CharacterSprite and all walk frames.", this);
                return;
            }
            previousPosition = MotionTarget.position;
            SetIdle();
        }

        private void LateUpdate()
        {
            if (!ready || MotionTarget == null || characterSprite == null) return;
            Vector3 position = MotionTarget.position;
            float distance = Vector3.Distance(position, previousPosition);
            previousPosition = position;
            if (terrainMovement != null && !terrainMovement.MovementEnabled)
            {
                SetIdle();
                return;
            }
            float dt = Time.deltaTime;
            if (dt <= 0f) return;
            if (distance > movementThreshold * dt)
            {
                if (graceRemaining <= 0f)
                    frameIndex = (frameIndex + 1) % walkFrames.Length;
                graceRemaining = movementGracePeriod;
            }
            else
            {
                graceRemaining -= dt;
                if (graceRemaining <= 0f)
                {
                    SetIdle();
                    return;
                }
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
            graceRemaining = 0f;
            frameClock = 0f;
            frameIndex = Mathf.Clamp(idleFrameIndex, 0, walkFrames.Length - 1);
            characterSprite.sprite = walkFrames[frameIndex];
        }
    }
}
