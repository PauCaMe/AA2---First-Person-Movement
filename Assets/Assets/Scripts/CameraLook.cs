using UnityEngine;

public class CameraLook : MonoBehaviour
{
    public Transform playerBody;
    public float mouseSensitivity = 100f;
    public float smoothTime = 0.05f;

    float targetYaw;
    float targetPitch;
    float currentYaw;
    float currentPitch;
    float yawVelocity;
    float pitchVelocity;

    public float minVerticalAngle = -60f;
    public float maxVerticalAngle = 60f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        currentYaw = playerBody.eulerAngles.y;
        targetYaw = currentYaw;
        currentPitch = 0f;
        targetPitch = 0f;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Objetivo de rotación según ratón
        targetYaw += mouseX;
        targetPitch -= mouseY;
        targetPitch = Mathf.Clamp(targetPitch, minVerticalAngle, maxVerticalAngle);

        // Suavizado
        currentYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, smoothTime);

        // Aplicar rotaciones
        playerBody.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}
