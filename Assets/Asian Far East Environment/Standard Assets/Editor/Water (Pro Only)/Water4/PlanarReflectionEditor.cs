using UnityEditor;
using UnityEngine;
using UnityStandardAssets.Water;

[CustomEditor(typeof(PlanarReflection))]
public class PlanarReflectionEditor : Editor
{
    private SerializedObject serObj;
    private SerializedProperty textureSize;
    private SerializedProperty clipPlaneOffset;
    private SerializedProperty reflectionPlane;
    private SerializedProperty reflectionCamera;

    void OnEnable()
    {
        serObj = new SerializedObject(target);
        textureSize = serObj.FindProperty("textureSize");
        clipPlaneOffset = serObj.FindProperty("clipPlaneOffset");
        reflectionPlane = serObj.FindProperty("reflectionPlane");
        reflectionCamera = serObj.FindProperty("reflectionCamera");
    }

    public override void OnInspectorGUI()
    {
        serObj.Update();
        EditorGUILayout.PropertyField(textureSize, new GUIContent("Texture Size"));
        EditorGUILayout.PropertyField(clipPlaneOffset, new GUIContent("Clip Plane Offset"));
        EditorGUILayout.PropertyField(reflectionPlane, new GUIContent("Reflection Plane"));
        EditorGUILayout.PropertyField(reflectionCamera, new GUIContent("Reflection Camera"));
        serObj.ApplyModifiedProperties();
    }
}