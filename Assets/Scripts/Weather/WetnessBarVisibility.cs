using UnityEngine;
using Encumbrance.Weather;

public class WetnessBarVisibility : MonoBehaviour
{
    [SerializeField] private WeatherController weather;
    [SerializeField] private GameObject wetnessBar;

    private void OnEnable() => Refresh();
    private void LateUpdate() => Refresh();

    private void Refresh()
    {
        if (weather == null || wetnessBar == null)
            return;

        bool isRaining =
            weather.EffectiveWeather == WeatherMode.HeavyRain ||
            weather.EffectiveWeather == WeatherMode.Storm;

        if (wetnessBar.activeSelf != isRaining)
            wetnessBar.SetActive(isRaining);
    }
}
