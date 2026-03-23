using UnityEngine;
using System.Collections;

public class WeatherController : MonoBehaviour
{
    public enum Weather { Sunny, Rainy, Foggy }
    public Weather currentWeather;

    public ParticleSystem rain;
    public Light sunLight;
    public float fogDensity = 0.08f;

    [Header("批量湿润控制")]
    public Transform floorParent;
    public Transform bridgeParent;
    public Terrain terrain;

    [Header("独立湿润材质")]
    public Material floorWetMaterial;
    public Material bridgeWetMaterial;
    public Material terrainWetMaterial;

    [Header("渐变速度")]
    public float transitionSpeed = 1f;

    [Header("昼夜循环设置")]
    public float dayDuration = 60f;
    public float timeOfDay;

    [Header("下雨声效")]
    public AudioSource rainAudio;
    public float rainVolume = 0.6f;
    public float audioFadeSpeed = 2f;

    private float sunnyIntensity = 1.2f;
    private Material[] defaultFloorMaterials;
    private Material[] defaultBridgeMaterials;
    private Material defaultTerrainMaterial;
    private Coroutine currentTransition;
    private Coroutine audioFadeCoroutine;

    void Start()
    {
        if (floorParent != null)
        {
            Renderer[] rs = floorParent.GetComponentsInChildren<Renderer>(true);
            defaultFloorMaterials = new Material[rs.Length];
            for (int i = 0; i < rs.Length; i++)
                defaultFloorMaterials[i] = rs[i].material;
        }

        if (bridgeParent != null)
        {
            Renderer[] bridgeRenderers = bridgeParent.GetComponentsInChildren<Renderer>(true);
            defaultBridgeMaterials = new Material[bridgeRenderers.Length];
            for (int i = 0; i < bridgeRenderers.Length; i++)
                defaultBridgeMaterials[i] = bridgeRenderers[i].material;
        }

        if (terrain != null)
            defaultTerrainMaterial = terrain.materialTemplate;

        if (rainAudio != null)
        {
            rainAudio.loop = true;
            rainAudio.volume = 0;
        }

        SetWeather(Weather.Sunny);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeather(Weather.Sunny);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeather(Weather.Rainy);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeather(Weather.Foggy);

        DayCycleUpdate();
    }

    void DayCycleUpdate()
    {
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay > 1) timeOfDay -= 1;

        float xRotation = Mathf.Lerp(0, 180, timeOfDay);
        sunLight.transform.rotation = Quaternion.Euler(xRotation, 120f, 0);

        float brightness = Mathf.Sin(timeOfDay * Mathf.PI);

        if (currentWeather == Weather.Sunny)
        {
            sunLight.intensity = brightness * sunnyIntensity;
            sunLight.shadowStrength = Mathf.Lerp(0.3f, 0.8f, brightness);

            if (timeOfDay < 0.25f)
                sunLight.color = Color.Lerp(new Color(1, 0.8f, 0.6f), Color.white, timeOfDay / 0.25f);
            else if (timeOfDay > 0.75f)
                sunLight.color = Color.Lerp(Color.white, new Color(1, 0.65f, 0.5f), (timeOfDay - 0.75f) / 0.25f);
            else
                sunLight.color = Color.white;
        }
        else if (currentWeather == Weather.Rainy)
        {
            sunLight.intensity = brightness * 0.15f;
            sunLight.shadowStrength = 0.2f;
            sunLight.color = Color.Lerp(Color.gray, Color.white, brightness);
        }
        else if (currentWeather == Weather.Foggy) // ✅ 这里修复了！
        {
            sunLight.intensity = 0.7f;
            sunLight.color = new Color(0.85f, 0.85f, 0.9f);
            sunLight.shadowStrength = 0.4f;
        }
    }

    void SetWeather(Weather weather)
    {
        currentWeather = weather;

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        if (rain != null) { rain.Stop(); rain.Clear(); }
        RenderSettings.fog = false;

        SetAllDry();

        if (audioFadeCoroutine != null)
            StopCoroutine(audioFadeCoroutine);

        switch (weather)
        {
            case Weather.Sunny:
                sunLight.gameObject.SetActive(true);
                if (rainAudio != null)
                    audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudio, 0f, audioFadeSpeed));
                break;

            case Weather.Rainy:
                sunLight.gameObject.SetActive(true);
                currentTransition = StartCoroutine(GradualWet());

                if (rain != null) rain.Play();
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = 0.0004f;
                RenderSettings.fogColor = new Color(0.94f, 0.94f, 0.94f);

                if (rainAudio != null)
                {
                    if (!rainAudio.isPlaying) rainAudio.Play();
                    audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudio, rainVolume, audioFadeSpeed));
                }
                break;

            case Weather.Foggy:
                sunLight.gameObject.SetActive(true);
                sunLight.intensity = 0.7f;
                sunLight.color = new Color(0.85f, 0.85f, 0.9f);
                sunLight.shadowStrength = 0.4f;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 30f;
                RenderSettings.fogEndDistance = 200f;
                RenderSettings.fogDensity = 0.08f;
                RenderSettings.fogColor = new Color(0.7f, 0.72f, 0.75f);

                if (rainAudio != null)
                    audioFadeCoroutine = StartCoroutine(FadeAudio(rainAudio, 0f, audioFadeSpeed));
                break;
        }
    }

    IEnumerator FadeAudio(AudioSource audio, float targetVol, float speed)
    {
        if (audio == null) yield break;

        while (Mathf.Abs(audio.volume - targetVol) > 0.01f)
        {
            audio.volume = Mathf.MoveTowards(audio.volume, targetVol, speed * Time.deltaTime);
            yield return null;
        }
        audio.volume = targetVol;
    }

    IEnumerator GradualWet()
    {
        if (floorParent != null && floorWetMaterial != null)
            foreach (Renderer r in floorParent.GetComponentsInChildren<Renderer>(true))
                r.material = floorWetMaterial;

        if (bridgeParent != null && bridgeWetMaterial != null)
            foreach (Renderer r in bridgeParent.GetComponentsInChildren<Renderer>(true))
                r.material = bridgeWetMaterial;

        if (terrain != null && terrainWetMaterial != null)
            terrain.materialTemplate = terrainWetMaterial;

        yield return null;
    }

    void SetAllDry()
    {
        if (floorParent != null && defaultFloorMaterials != null)
        {
            Renderer[] rs = floorParent.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rs.Length; i++)
                rs[i].material = defaultFloorMaterials[i];
        }

        if (bridgeParent != null && defaultBridgeMaterials != null)
        {
            Renderer[] rs = bridgeParent.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rs.Length; i++)
                rs[i].material = defaultBridgeMaterials[i];
        }

        if (terrain != null && defaultTerrainMaterial != null)
            terrain.materialTemplate = defaultTerrainMaterial;
    }
}