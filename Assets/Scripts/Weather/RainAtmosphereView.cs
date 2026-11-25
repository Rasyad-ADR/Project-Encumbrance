using UnityEngine;
using UnityEngine.UI;

namespace Encumbrance.Weather
{
    [RequireComponent(typeof(RawImage))]
    public sealed class RainAtmosphereView : MonoBehaviour
    {
        [SerializeField] private WeatherController weather;
        [SerializeField, Range(0,1)] private float heavyRainOpacity = .18f;
        [SerializeField, Range(0,1)] private float stormOpacity = .32f;
        [SerializeField] private Vector2 uvSpeed = new Vector2(.35f,.5f);
        [SerializeField] private Vector2 tiling = new Vector2(1,1);
        [SerializeField, Min(.01f)] private float fadeSeconds = .7f;
        private RawImage image;
        private Vector2 offset;
        private float alpha;
        private void Awake()
        {
            image = GetComponent<RawImage>();
            image.raycastTarget = false;
            ApplyAlpha(0);
        }
        private void Update()
        {
            var mode = weather != null ? weather.EffectiveWeather : WeatherMode.Dry;
            float target = mode == WeatherMode.Storm ? stormOpacity : mode == WeatherMode.HeavyRain ? heavyRainOpacity : 0;
            alpha = Mathf.MoveTowards(alpha,target,Time.unscaledDeltaTime / Mathf.Max(.01f,fadeSeconds));
            if (alpha > 0)
            {
                offset += uvSpeed * Time.deltaTime * (mode == WeatherMode.Storm ? 1.5f : 1f);
                offset.x = Mathf.Repeat(offset.x,1); offset.y = Mathf.Repeat(offset.y,1);
                image.uvRect = new Rect(offset,tiling);
            }
            ApplyAlpha(alpha);
        }
        private void ApplyAlpha(float value) { var c = image.color; c.a = value; image.color = c; }
        private void OnDisable() { alpha = 0; if (image != null) ApplyAlpha(0); }
    }
}
