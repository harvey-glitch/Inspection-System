using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("Normal walking speed of the player.")]
    public float MovementSpeed = 4.0f;

    [Tooltip("How far the player can look up or down.")]
    public float RotationAngleLimit = 65.0f;

    [Tooltip("How fast the camera rotate around.")]
    public float RotationSpeed = 3.0f;

    [Tooltip("Makes gravity stronger or weaker.")]
    public float GravityMultiplier = 2.0f;

    [Space(5)]
    [Header("Ground Check Settings")]

    [Tooltip("Point used to check if the player is on the ground.")]
    public Transform SphereTransform;

    [Tooltip("Size of the sphere used for checking ground contact.")]
    public float SphereRadius = 0.5f;

    [Tooltip("Offset checking for ground, useful for stairs.")]
    public float GroundedOffset = 0.14f;

    [Tooltip("Shows if the player is touching the ground.")]
    public bool IsGrounded;

    // Reference to the CharacterController component for movement
    private CharacterController _characterController;

    // Reference to the main camera for handling rotations
    private Camera _camera;

    // Stores the current vertical and horizontal angles for rotation
    private float _pitch, _yaw;

    // variable for smoothing movement and rotation inputs
    private Vector3 _smoothLookInput;

    // Tracks the player's current vertical velocity (used for gravity and jumping)
    private Vector3 _verticalVelocity;

    private void Awake()
    {
        _characterController ??= GetComponent<CharacterController>();
        _camera = Camera.main;
    }

    private void Start()
    {
        // hide the cursor at the start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        GroundCheck();

        HandleMove();
        HandleRotate();
        ApplyGravity();
    }

    private void HandleMove()
    {
        // normalize the input to avoid faster movement when moving diagonally
        Vector3 movementInput = GetMovementInput().normalized * MovementSpeed;

        // only move when theres a valid input
        if (movementInput.sqrMagnitude >= 0.0001f)
        {
            // create a direction based on orientation and input
            Vector3 movement = transform.right * movementInput.x + transform.forward * movementInput.z;

            _characterController.Move(movement * Time.deltaTime);
        }
    }

    private void HandleRotate()
    {
        Vector2 rotationInput = GetRotationInput() * RotationSpeed;

        // only rotate when theres a valid input
        if (rotationInput.sqrMagnitude >= 0.0001f)
        {
            _yaw += rotationInput.x;  // accumulate horizontal rotation turning
            _pitch -= rotationInput.y; // accumulate vertical rotation for looking up and down

            // clamp the vertical rotation to avoid camera flipping
            _pitch = Mathf.Clamp(_pitch, -RotationAngleLimit, RotationAngleLimit);

            // apply the horizontal and vertical rotation
            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            _camera.transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }

    private void GroundCheck()
    {
        Vector3 sphereOrigin = SphereTransform.position + Vector3.down * GroundedOffset;

        // check if the character is grounded
        IsGrounded = Physics.CheckSphere(sphereOrigin, SphereRadius);
    }

    private void ApplyGravity()
    {
        // push the character to the ground
        if (IsGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -2f;
        }

        if (!IsGrounded)
        {
            // only apply gravity when not grounded
            _verticalVelocity.y += -9.81f * GravityMultiplier * Time.deltaTime;
        }

        _characterController.Move(_verticalVelocity * Time.deltaTime);
    }

    #region Input Methods
    private Vector3 GetMovementInput()
    {
        return new Vector3(
            Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    }

    private Vector3 GetRotationInput()
    {
        Vector3 lookInput = new Vector2(
            Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // use smoothdamp for smooth transition with easing effect
        _smoothLookInput = Vector3.Lerp(
            _smoothLookInput, lookInput, Time.deltaTime * 10f);

        return _smoothLookInput;
    }
    #endregion

    #region For Debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Vector3 spherOrigin = SphereTransform.position + Vector3.down * GroundedOffset;

        Gizmos.DrawWireSphere(spherOrigin, SphereRadius);
    }
    #endregion
}