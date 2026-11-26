using UnityEngine;
using Encumbrance.Weather;

namespace Encumbrance.Resonance
{
    public enum HazardLayer { Bottom, Middle, Final }

    public sealed class HazardController : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float totalWeight = 60f;
        [SerializeField] private HazardLayer currentLayer = HazardLayer.Bottom;
        [SerializeField, Min(0.01f)] private float weightDivisor = 10f;
        [SerializeField] private float bottomMultiplier = 0.75f;
        [SerializeField] private float middleMultiplier = 1.0f;
        [SerializeField] private float finalMultiplier = 1.5f;

        [Header("Weather Penalty")]
        [SerializeField] private WeatherController weatherController;
        [SerializeField, Min(1f)] private float heavyRainMultiplier = 1.2f;
        [SerializeField, Min(1f)] private float stormMultiplier = 1.5f;

        private float GetWeatherMultiplier()
        {
            if (weatherController == null) return 1f;

            switch (weatherController.EffectiveWeather)
            {
                case WeatherMode.HeavyRain: return heavyRainMultiplier;
                case WeatherMode.Storm: return stormMultiplier;
                default: return 1f;
            }
        }

        private float GetMultiplier()
        {
            switch (currentLayer)
            {
                case HazardLayer.Bottom: return bottomMultiplier;
                case HazardLayer.Middle: return middleMultiplier;
                default: return finalMultiplier;
            }
        }

        public float GetMistakePenalty() => totalWeight * GetMultiplier() * GetWeatherMultiplier() / weightDivisor;

        public void SetWeight(float weight) => totalWeight = Mathf.Max(0f, weight);
        public void SetLayer(HazardLayer layer) => currentLayer = layer;
    }
}