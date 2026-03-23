using UnityEngine;

namespace UnityStandardAssets.Water
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class PlanarReflection : MonoBehaviour
    {
        public Camera reflectionCamera;
        public Transform reflectionPlane;
        public int textureSize = 512;
        public float clipPlaneOffset = 0.07f;

        private RenderTexture reflectionTexture;
        private Camera mainCamera;

        void Start()
        {
            mainCamera = GetComponent<Camera>();
            CreateReflectionTexture();
        }

        void OnWillRenderObject()
        {
            if (!enabled || !reflectionCamera || !reflectionPlane)
                return;

            RenderReflection();
        }

        void CreateReflectionTexture()
        {
            if (reflectionTexture) DestroyImmediate(reflectionTexture);
            reflectionTexture = new RenderTexture(textureSize, textureSize, 16);
            reflectionTexture.isPowerOfTwo = true;
            reflectionTexture.hideFlags = HideFlags.DontSave;
            Shader.SetGlobalTexture("_PlanarReflectionTexture", reflectionTexture);
        }

        void RenderReflection()
        {
            Vector3 planePos = reflectionPlane.position;
            Vector3 planeNormal = reflectionPlane.up;

            float d = -Vector3.Dot(planeNormal, planePos) - clipPlaneOffset;
            Vector4 reflectionPlaneVec = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, d);

            Matrix4x4 reflection = Matrix4x4.zero;
            CalculateReflectionMatrix(ref reflection, reflectionPlaneVec);
            Vector3 newPos = reflection.MultiplyPoint(mainCamera.transform.position);

            reflectionCamera.worldToCameraMatrix = mainCamera.worldToCameraMatrix * reflection;
            Vector4 clipPlane = CameraSpacePlane(reflectionCamera, planePos, planeNormal, 1.0f);
            reflectionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlane);

            reflectionCamera.transform.position = newPos;
            Vector3 euler = mainCamera.transform.eulerAngles;
            reflectionCamera.transform.eulerAngles = new Vector3(-euler.x, euler.y, euler.z);

            GL.invertCulling = true;
            reflectionCamera.targetTexture = reflectionTexture;
            reflectionCamera.Render();
            GL.invertCulling = false;
        }

        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        private static Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * 0.07f;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }
    }
}