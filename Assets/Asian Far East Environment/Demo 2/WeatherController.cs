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
    public Transform floorParent;       // 地面父物体
    public Transform bridgeParent;      // 桥父物体
    public Terrain terrain;             // 地形

    [Header("独立湿润材质")]
    public Material floorWetMaterial;   // 地面湿润材质
    public Material bridgeWetMaterial;  // 桥湿润材质（独立！）
    public Material terrainWetMaterial; // 地形湿润材质

    [Header("渐变速度")]
    public float transitionSpeed = 1f;

    [Header("昼夜循环设置")]
    public float dayDuration = 60f;
    public float timeOfDay;

    private float sunnyIntensity = 1.2f;
    private Material[] defaultFloorMaterials;
    private Material[] defaultBridgeMaterials;
    private Material defaultTerrainMaterial;
    private Coroutine currentTransition;

    void Start()
    {
        // 保存地面原始材质
        if (floorParent != null)
        {
            Renderer[] rs = floorParent.GetComponentsInChildren<Renderer>(true);
            defaultFloorMaterials = new Material[rs.Length];
            for (int i = 0; i < rs.Length; i++)
                defaultFloorMaterials[i] = rs[i].material;
        }

        // 保存桥原始材质
        if (bridgeParent != null)
        {
            Renderer[] bridgeRenderers = bridgeParent.GetComponentsInChildren<Renderer>(true);
            defaultBridgeMaterials = new Material[bridgeRenderers.Length];
            for (int i = 0; i < bridgeRenderers.Length; i++)
                defaultBridgeMaterials[i] = bridgeRenderers[i].material;
        }

        // 保存地形材质
        if (terrain != null)
            defaultTerrainMaterial = terrain.materialTemplate;

        SetWeather(Weather.Sunny);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetWeather(Weather.Sunny);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetWeather(Weather.Rainy);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetWeather(Weather.Foggy);

        if (currentWeather == Weather.Sunny)
            DayCycleUpdate();
    }

    void DayCycleUpdate()
    {
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay > 1) timeOfDay -= 1;

        float xRotation = Mathf.Lerp(0, 180, timeOfDay);
        sunLight.transform.rotation = Quaternion.Euler(xRotation, 120f, 0);

        float brightness = Mathf.Sin(timeOfDay * Mathf.PI);
        sunLight.intensity = brightness * sunnyIntensity;

        if (timeOfDay < 0.25f)
            sunLight.color = Color.Lerp(new Color(1, 0.8f, 0.6f), Color.white, timeOfDay / 0.25f);
        else if (timeOfDay > 0.75f)
            sunLight.color = Color.Lerp(Color.white, new Color(1, 0.65f, 0.5f), (timeOfDay - 0.75f) / 0.25f);
        else
            sunLight.color = Color.white;

        sunLight.shadowStrength = Mathf.Lerp(0.3f, 0.8f, brightness);
    }

    void SetWeather(Weather weather)
    {
        currentWeather = weather;

        if (currentTransition != null)
            StopCoroutine(currentTransition);

        if (rain != null) { rain.Stop(); rain.Clear(); }
        RenderSettings.fog = false;
        if (sunLight != null) sunLight.gameObject.SetActive(true);

        // 全部还原干燥
        SetAllDry();

        switch (weather)
        {
            case Weather.Sunny:
                break;

            case Weather.Rainy:
                if (sunLight != null) sunLight.gameObject.SetActive(false);
                currentTransition = StartCoroutine(GradualWet());

                if (rain != null) rain.Play();
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogDensity = 0.0009f;
                RenderSettings.fogColor = new Color(0.94f, 0.94f, 0.94f);
                break;

            case Weather.Foggy:
                sunLight.intensity = 0.7f;
                sunLight.color = new Color(0.85f, 0.85f, 0.9f);
                sunLight.shadowStrength = 0.4f;
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 20f;
                RenderSettings.fogEndDistance = 120f;
                RenderSettings.fogDensity = 0.12f;
                RenderSettings.fogColor = new Color(0.7f, 0.72f, 0.75f);
                break;
        }
    }

    // 核心：分别给地面、桥、地形 用不同材质变湿
    IEnumerator GradualWet()
    {
        // 地面变湿
        if (floorParent != null && floorWetMaterial != null)
            foreach (Renderer r in floorParent.GetComponentsInChildren<Renderer>(true))
                r.material = floorWetMaterial;

        // 桥变湿（独立材质！）
        if (bridgeParent != null && bridgeWetMaterial != null)
            foreach (Renderer r in bridgeParent.GetComponentsInChildren<Renderer>(true))
                r.material = bridgeWetMaterial;

        // 地形变湿
        if (terrain != null && terrainWetMaterial != null)
            terrain.materialTemplate = terrainWetMaterial;

        yield return null;
    }

    // 全部还原干燥
    void SetAllDry()
    {
        // 还原地面
        if (floorParent != null && defaultFloorMaterials != null)
        {
            Renderer[] rs = floorParent.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rs.Length; i++)
                rs[i].material = defaultFloorMaterials[i];
        }

        // 还原桥
        if (bridgeParent != null && defaultBridgeMaterials != null)
        {
            Renderer[] rs = bridgeParent.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rs.Length; i++)
                rs[i].material = defaultBridgeMaterials[i];
        }

        // 还原地形
        if (terrain != null && defaultTerrainMaterial != null)
            terrain.materialTemplate = defaultTerrainMaterial;
    }
}