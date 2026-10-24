using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float velocity = 5f;
    public float sprintMultiplier = 2f;
    public float jumpForce = 1f;
    public float gravity = -15f;

    public InputAction moveAction;
    public InputAction sprintAction;
    public InputAction jumpAction;

    private CharacterController controller;
    private float verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();
        jumpAction.Disable();
    }

    void Update()
    {

        Vector2 input = moveAction.ReadValue<Vector2>();

        float currentVelocity = velocity;
        if (sprintAction.IsPressed())
        {
            currentVelocity *= sprintMultiplier; // Double the speed when sprinting
        }

        Vector3 move = (transform.right * input.x + transform.forward * input.y) * currentVelocity;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // Small negative value to keep the player grounded
        }
        if (controller.isGrounded && jumpAction.WasPressedThisFrame())
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

    }
}