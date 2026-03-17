using UnityEngine;

public class WeatherController : MonoBehaviour
{
    // 天气类型
    public enum Weather { Sunny, Rainy, Foggy }
    public Weather currentWeather;

    // 下雨
    public ParticleSystem rain;

    // 雾
    private float normalFog;
    [Header("雾浓度（0.05~0.1最合适）")]
    public float fogDensity = 0.01f;

    void Start()
    {
        // 保存原始雾浓度
        normalFog = RenderSettings.fogDensity;

        // 默认晴天
        SetWeather(Weather.Sunny);
    }

    void Update()
    {
        // 按键切换
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeather(Weather.Sunny);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeather(Weather.Rainy);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeather(Weather.Foggy);
    }

    void SetWeather(Weather weather)
    {
        currentWeather = weather;

        switch (weather)
        {
            case Weather.Sunny:
                // 停雨
                if (rain != null && rain.isPlaying) rain.Stop();
                // 关雾 ✅ 修复这里
                RenderSettings.fog = false;
                RenderSettings.fogDensity = 0;
                break;

            case Weather.Rainy:
                // 下雨
                if (rain != null && !rain.isPlaying) rain.Play();
                // 关雾 ✅ 修复这里
                RenderSettings.fog = false;
                RenderSettings.fogDensity = 0;
                break;

            case Weather.Foggy:
                // 停雨
                if (rain != null && rain.isPlaying) rain.Stop();
                // 开雾
                RenderSettings.fog = true;
                RenderSettings.fogDensity = fogDensity;
                break;
        }
    }
}