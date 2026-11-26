using UnityEngine;
using UnityEngine.InputSystem;

public class Camara : MonoBehaviour
{
    public float sensitivity = 0.1f;

    public InputAction lookAction;
    public Transform playerBody;

    private float xRotation = 0f;

    private void OnEnable()
    {
        lookAction.Enable();
    }

    private void Disable()
    {
        lookAction.Disable();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();

        float mouseX = look.x * sensitivity;
        float mouseY = look.y * sensitivity;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        playerBody.Rotate(Vector3.up * mouseX);
    }
}
