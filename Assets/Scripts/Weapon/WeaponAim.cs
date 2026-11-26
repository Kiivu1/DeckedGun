using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryCard inventory;
    [SerializeField] private Camera playerCamera;

    [Header("Aim")]
    [SerializeField] private Vector3 aimPosition = new Vector3(0f, -0.12f, 0.5f);
    [SerializeField, Range(1f, 179f)] private float aimFov = 45f;
    [SerializeField, Min(0.1f)] private float transitionSpeed = 12f;

    private Vector3 normalPosition;
    private float normalFov;
    private bool initialized;

    void Start()
    {
        normalPosition = transform.localPosition;
        if (playerCamera != null) normalFov = playerCamera.fieldOfView;
        initialized = true;
    }

    void Update()
    {
        if (inventory == null || !inventory.isActiveAndEnabled || Time.timeScale == 0f)
        {
            RestorePose();
            return;
        }

        bool aiming = Mouse.current != null && Mouse.current.rightButton.isPressed;
        float blend = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition, aiming ? aimPosition : normalPosition, blend);

        if (playerCamera != null)
            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView, aiming ? aimFov : normalFov, blend);
    }

    void OnDisable()
    {
        RestorePose();
    }

    void RestorePose()
    {
        if (!initialized) return;
        transform.localPosition = normalPosition;
        if (playerCamera != null) playerCamera.fieldOfView = normalFov;
    }
}