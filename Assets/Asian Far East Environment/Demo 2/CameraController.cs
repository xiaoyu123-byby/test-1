using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float upDownSpeed = 3f; // 上升下降速度

    [Header("视角旋转设置")]
    public float lookSensitivity = 2f;
    public float verticalLookLimit = 80f;

    private float rotationX = 0f;

    void Update()
    {
        // ======================
        // 鼠标 右键 按住 控制视角
        // ======================
        if (Input.GetMouseButton(1)) // 这里改成了右键 1 = 右键
        {
            // 获取鼠标移动
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            // 上下视角限制（防止转圈）
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -verticalLookLimit, verticalLookLimit);

            // 应用旋转
            transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y + mouseX, 0);
        }

        // ======================
        // WASD 控制前后左右移动
        // ======================
        float h = Input.GetAxisRaw("Horizontal"); // A D
        float v = Input.GetAxisRaw("Vertical");   // W S

        Vector3 dir = transform.right * h + transform.forward * v;
        dir.y = 0; // 保持水平移动，不飞起来
        dir.Normalize();

        transform.Translate(dir * moveSpeed * Time.deltaTime, Space.World);

        // ======================
        // E 上升 | Q 下降
        // ======================
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * upDownSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(Vector3.down * upDownSpeed * Time.deltaTime, Space.World);
        }
    }
}