using UnityEngine;
using UnityStandardAssets.Water;

[RequireComponent(typeof(WaterBase))]
public class WaterTile : MonoBehaviour
{
    public PlanarReflection reflection;
    public WaterBase waterBase;

    void Start()
    {
        AcquireComponents();
    }

    void AcquireComponents()
    {
        if (!reflection)
            reflection = GetComponent<PlanarReflection>();
        if (!waterBase)
            waterBase = GetComponent<WaterBase>();
    }

#if UNITY_EDITOR
    void Update()
    {
        AcquireComponents();
    }
#endif
}