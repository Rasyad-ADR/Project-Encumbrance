using UnityEngine;

namespace Encumbrance.World
{
    public sealed class TerrainRecycler : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform[] segments;
        [SerializeField, Min(0.1f)] private float segmentLength = 12f;
        [SerializeField, Min(0f)] private float offscreenMargin = 0.1f;

        private void Awake()
        {
            if (gameplayCamera == null)
                Debug.LogError("TerrainRecycler: assign Gameplay Camera.", this);

            if (segments == null || segments.Length < 2)
                Debug.LogError("TerrainRecycler: assign at least two segments.", this);
        }

        private void LateUpdate()
        {
            if (gameplayCamera == null || segments == null || segments.Length < 2)
                return;

            // Move each segment after it has completely left the camera on the left.
            // Designed for equal-width Unity Cubes scrolling in negative local X.
            for (int i = 0; i < segments.Length; i++)
            {
                Transform segment = segments[i];
                if (segment == null)
                    continue;

                // Unity's built-in Cube spans -0.5..+0.5 in local X.
                // TransformPoint includes the cube's scale and the parent's tilt.
                Vector3 rightEdge = segment.TransformPoint(new Vector3(0.5f, 0f, 0f));
                Vector3 viewportPoint = gameplayCamera.WorldToViewportPoint(rightEdge);

                if (viewportPoint.z <= 0f || viewportPoint.x >= -offscreenMargin)
                    continue;

                float rightmostCenter = float.NegativeInfinity;
                for (int j = 0; j < segments.Length; j++)
                {
                    if (segments[j] != null)
                        rightmostCenter = Mathf.Max(rightmostCenter, segments[j].localPosition.x);
                }

                Vector3 position = segment.localPosition;
                position.x = rightmostCenter + segmentLength;
                segment.localPosition = position;
            }
        }
    }
}
