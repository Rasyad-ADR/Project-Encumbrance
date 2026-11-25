using System.Collections.Generic;
using Encumbrance.Resonance;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Encumbrance.Hazard
{
    public sealed class RainHazardController : MonoBehaviour
    {
        [SerializeField] private ResonanceController resonance;
        [SerializeField] private JournalWetness wetness;
        [SerializeField] private RectTransform dropletContainer;
        [SerializeField] private Sprite[] dropletSprites;
        [SerializeField] private Color dropletTint = Color.white;
        [SerializeField, Min(0)] private float startDelay = 5f;
        [SerializeField, Min(0.05f)] private float minSpawnInterval = 2f;
        [SerializeField, Min(0.05f)] private float maxSpawnInterval = 3f;
        [SerializeField, Min(0.1f)] private float dropletLifetime = 5f;
        [SerializeField, Min(1)] private int maxActiveDroplets = 8;
        [SerializeField, Min(1)] private float dropletSize = 40f;
        [SerializeField] private bool rainEnabled = true;
        private readonly List<RainDroplet> active = new List<RainDroplet>();
        private float timer;
        private bool wasHeld;
        private Vector2 previous;
        private Canvas canvas;
        private void Awake()
        {
            if (resonance == null || wetness == null || dropletContainer == null)
            {
                Debug.LogError("RainHazardController: assign Resonance, Wetness and Droplet Container.", this);
                enabled = false;
                return;
            }
            canvas = dropletContainer.GetComponentInParent<Canvas>();
            if (canvas == null) { Debug.LogError("Rain: container needs a Canvas.", this); enabled = false; }
            timer = startDelay;
        }
        public void SetRainEnabled(bool value)
        {
            rainEnabled = value;
            if (!value) Clear();
        }
        private void OnDisable() => Clear();
        private void Clear()
        {
            foreach (var drop in active) if (drop != null) Destroy(drop.gameObject);
            active.Clear();
            wasHeld = false;
            timer = startDelay;
        }
        private void Update()
        {
            if (!rainEnabled || !resonance.IsAscentActive)
            {
                Clear();
                return;
            }
            var mouse = Mouse.current;
            bool held = mouse != null && mouse.leftButton.isPressed;
            Vector2 position = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
            bool sweeping = held && wasHeld && Time.deltaTime > 0f;
            Camera camera = canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.rootCanvas.worldCamera;
            for (int i = active.Count - 1; i >= 0; i--)
            {
                var drop = active[i];
                if (drop == null) { active.RemoveAt(i); continue; }
                bool wiped = sweeping && drop.Crossed(previous, position, camera);
                if (!wiped && !drop.Tick(Time.deltaTime)) continue;
                active.RemoveAt(i);
                Destroy(drop.gameObject);
                if (wiped) wetness.Wiped(); else wetness.Missed();
            }
            previous = position;
            wasHeld = held;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                if (active.Count < maxActiveDroplets) Spawn();
                timer = Random.Range(minSpawnInterval, Mathf.Max(minSpawnInterval, maxSpawnInterval));
            }
        }
        private void Spawn()
        {
            Rect bounds = dropletContainer.rect;
            if (bounds.width < dropletSize || bounds.height < dropletSize) return;
            GameObject go = new GameObject("RainDroplet", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RainDroplet));
            RectTransform rect = (RectTransform)go.transform;
            rect.SetParent(dropletContainer, false);
            rect.anchorMin = rect.anchorMax = dropletContainer.pivot;
            rect.pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = dropletSprites != null && dropletSprites.Length > 0 ? dropletSprites[Random.Range(0, dropletSprites.Length)] : null;
            Vector2 size = Vector2.one * dropletSize;
            if (sprite != null) size = sprite.rect.size / Mathf.Max(sprite.rect.width, sprite.rect.height) * dropletSize;
            rect.sizeDelta = size;
            Vector2 half = size * 0.5f;
            rect.anchoredPosition = new Vector2(Random.Range(bounds.xMin + half.x, bounds.xMax - half.x), Random.Range(bounds.yMin + half.y, bounds.yMax - half.y));
            var drop = go.GetComponent<RainDroplet>();
            drop.Initialize(dropletLifetime, sprite, dropletTint);
            active.Add(drop);
        }
    }
}
