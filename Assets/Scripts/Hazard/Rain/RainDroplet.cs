using UnityEngine;
using UnityEngine.UI;

namespace Encumbrance.Hazard
{
    [RequireComponent(typeof(Image))]
    public sealed class RainDroplet : MonoBehaviour
    {
        private RectTransform rect;
        private Image image;
        private float remaining;
        private float elapsed;
        private Color tint;
        public void Initialize(float lifetime, Sprite sprite, Color color)
        {
            rect = (RectTransform)transform;
            image = GetComponent<Image>();
            remaining = lifetime;
            tint = color;
            image.sprite = sprite;
            image.raycastTarget = false;
            image.color = new Color(color.r, color.g, color.b, 0);
        }
        public bool Tick(float dt)
        {
            elapsed += dt;
            remaining -= dt;
            Color color = tint;
            color.a *= Mathf.Clamp01(elapsed / 0.2f);
            image.color = color;
            return remaining <= 0f;
        }
        // Segment/rectangle intersection in the droplet's local UI space.
        public bool Crossed(Vector2 from, Vector2 to, Camera camera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, from, camera, out Vector2 a) ||
                !RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, to, camera, out Vector2 b)) return false;
            if ((b - a).sqrMagnitude < 0.0001f) return false;
            Rect r = rect.rect;
            Vector2 d = b - a;
            float lo = 0f, hi = 1f;
            return Clip(a.x, d.x, r.xMin, r.xMax, ref lo, ref hi) &&
                   Clip(a.y, d.y, r.yMin, r.yMax, ref lo, ref hi);
        }
        private static bool Clip(float p, float d, float min, float max, ref float lo, ref float hi)
        {
            if (Mathf.Abs(d) < 0.00001f) return p >= min && p <= max;
            float a = (min - p) / d, b = (max - p) / d;
            lo = Mathf.Max(lo, Mathf.Min(a, b));
            hi = Mathf.Min(hi, Mathf.Max(a, b));
            return lo <= hi;
        }
    }
}
