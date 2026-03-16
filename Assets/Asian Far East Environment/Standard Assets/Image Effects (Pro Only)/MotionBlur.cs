using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("Image Effects/Camera/Motion Blur")]
    public class MotionBlur : ImageEffectBase
    {
        [Tooltip("Rotation and movement will be scaled by this factor.")]
        public float movementScale = 0.0f;

        [Tooltip("Remove rotation effect from the motion blur calculation.")]
        public bool ignoreRotation = false;

        [Tooltip("Remove translation (position) effect from the motion blur calculation.")]
        public bool ignoreTranslation = false;

        [Tooltip("Use this if you want to use motion blur on a camera with a fixed projection (e.g. for rendering into a rendertexture).")]
        public bool useSolidAngle = true;

        private Matrix4x4 previousViewProjectionMatrix;
        private Vector3 previousCameraPosition;


        void OnEnable()
        {
            previousCameraPosition = GetComponent<Camera>().transform.position;
            previousViewProjectionMatrix = GetComponent<Camera>().projectionMatrix * GetComponent<Camera>().worldToCameraMatrix;
        }


        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            material.SetMatrix("_PreviousViewProjectionMatrix", previousViewProjectionMatrix);

            float w = source.width;
            float h = source.height;
            float z = GetComponent<Camera>().nearClipPlane;
            float yf = Mathf.Tan(GetComponent<Camera>().fieldOfView / 2 * Mathf.Deg2Rad) * z;
            float xf = yf * GetComponent<Camera>().aspect;

            Vector4 topLeft = new Vector4(-xf, yf, z, 0);
            Vector4 bottomRight = new Vector4(xf, -yf, z, 0);

            if (GetComponent<Camera>().orthographic)
            {
                topLeft = new Vector4(-GetComponent<Camera>().orthographicSize * GetComponent<Camera>().aspect, GetComponent<Camera>().orthographicSize, 0.0f, 0.0f);
                bottomRight = new Vector4(GetComponent<Camera>().orthographicSize * GetComponent<Camera>().aspect, -GetComponent<Camera>().orthographicSize, 0.0f, 0.0f);
            }

            material.SetVector("_TopLeft", topLeft);
            material.SetVector("_BottomRight", bottomRight);

            float scale = movementScale;
            if (ignoreRotation)
                material.DisableKeyword("MOTION_BLUR_ROTATION");
            else
                material.EnableKeyword("MOTION_BLUR_ROTATION");

            if (ignoreTranslation)
                material.DisableKeyword("MOTION_BLUR_TRANSLATION");
            else
                material.EnableKeyword("MOTION_BLUR_TRANSLATION");

            if (useSolidAngle)
                material.EnableKeyword("MOTION_BLUR_SOLID_ANGLE");
            else
                material.DisableKeyword("MOTION_BLUR_SOLID_ANGLE");

            material.SetFloat("_Scale", scale);

            Graphics.Blit(source, destination, material);

            Camera cam = GetComponent<Camera>();
            previousCameraPosition = cam.transform.position;
            previousViewProjectionMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;
        }
    }
}