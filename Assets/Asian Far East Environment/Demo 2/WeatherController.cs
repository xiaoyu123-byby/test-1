using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public enum Weather { Sunny, Rainy, Foggy }
    public Weather currentWeather;

    public ParticleSystem rain;
    public float fogDensity = 0.005f;

    void Start()
    {
        SetWeather(Weather.Sunny);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeather(Weather.Sunny);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeather(Weather.Rainy);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeather(Weather.Foggy);
        }
    }

    void SetWeather(Weather weather)
    {
        currentWeather = weather;

        // 强制清空所有天气
        if (rain != null)
        {
            rain.Stop();
            rain.Clear();
        }

        // 全局默认关闭雾
        RenderSettings.fog = false;
        RenderSettings.fogDensity = 0;

        switch (weather)
        {
            case Weather.Sunny:
                // 晴天：无效果
                break;

            case Weather.Rainy:
                // 雨天：下雨 + 【极淡 白色雾】
                if (rain != null)
                {
                    rain.Play();
                }

                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 60f;
                RenderSettings.fogEndDistance = 250f;
                RenderSettings.fogDensity = 0.0008f; // 超级淡
                RenderSettings.fogColor = Color.white; // 纯白色雾
                break;

            case Weather.Foggy:
                // 雾天：正常浓度雾
                RenderSettings.fog = true;
                RenderSettings.fogDensity = fogDensity;
                RenderSettings.fogColor = Color.white;
                break;
        }
    }
}