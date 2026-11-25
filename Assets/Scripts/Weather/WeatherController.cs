using Encumbrance.Hazard;
using Encumbrance.Resonance;
using UnityEngine;

namespace Encumbrance.Weather
{
    public enum WeatherMode { Dry, HeavyRain, Storm }

    public sealed class WeatherController : MonoBehaviour
    {
        [SerializeField] private WeatherMode weather = WeatherMode.HeavyRain;
        [SerializeField] private RainHazardController rainHazard;
        [SerializeField] private ResonanceController resonance;
        public WeatherMode Current => weather;
        public WeatherMode EffectiveWeather => isActiveAndEnabled && resonance != null && resonance.IsAscentActive
            ? weather : WeatherMode.Dry;
        private void Start() => Apply();
        private void Update() => Apply();
        public void SetWeather(WeatherMode value) { weather = value; Apply(); }
        public void SetDry() => SetWeather(WeatherMode.Dry);
        public void SetHeavyRain() => SetWeather(WeatherMode.HeavyRain);
        public void SetStorm() => SetWeather(WeatherMode.Storm);
        private bool? lastRain;
        private void Apply()
        {
            bool enabledRain = EffectiveWeather != WeatherMode.Dry;
            if (lastRain == enabledRain) return;
            lastRain = enabledRain;
            if (rainHazard != null) rainHazard.SetRainEnabled(enabledRain);
        }
        private void OnDisable()
        {
            lastRain = null;
            if (rainHazard != null) rainHazard.SetRainEnabled(false);
        }
    }
}
