using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    // Movement
    public float baseSpeed = 5f;
    public float sprintSpeedMult = 1.5f;
    public float crouchSpeedMult = 0.5f;

    // Jumping
    [SerializeField]
    public float jumpForce = 5f;
    [SerializeField]
    private bool isGrounded;

    // Crouching
    public float crouchHeight = 1f;
    private float standingHeight = 2f;
    private float crouchAnimSpeed = 15f;
    private Vector3 cameraStandingPos;
    private Vector3 cameraCrouchPos;

    // Groundcheck
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    // Networked states
    public NetworkVariable<bool> isCrouched = new NetworkVariable<bool>(false, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Look
    public float mouseSensitivity = 2f;

    // Physics
    public float clientPushForce = 25f;

    // Components
    private Rigidbody rb;
    public Camera playerCamera;
    public AudioListener audioListener;
    private CapsuleCollider capsuleCollider;

    // Input
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction crouchAction;


    private float xRotation = 0f;

    void Awake()
    {
        // Setup Movement
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("Dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        // Setup Look
        lookAction = new InputAction("Look", binding: "<Mouse>/delta");
        lookAction.AddBinding("<Gamepad>/rightStick");

        // Setup Actions
        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        jumpAction.AddBinding("<Gamepad>/buttonSouth");

        sprintAction = new InputAction("Sprint", binding: "<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");

        crouchAction = new InputAction("Crouch", binding: "<Keyboard>/leftCtrl");
        crouchAction.AddBinding("<Keyboard>/c");
        crouchAction.AddBinding("<Gamepad>/buttonEast");
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider>();

        // Cache camera positions for standing/crouching
        cameraStandingPos = playerCamera.transform.localPosition;
        cameraCrouchPos = new Vector3(cameraStandingPos.x, cameraStandingPos.y - (standingHeight - crouchHeight) / 2f, cameraStandingPos.z);
        standingHeight = capsuleCollider.height;
    }

    // Only enable inputs for the player actually controlling this object
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            moveAction.Enable();
            lookAction.Enable();
            jumpAction.Enable();
            sprintAction.Enable();
            crouchAction.Enable();

            // THIS player
            playerCamera.enabled = true;
            audioListener.enabled = true;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            {
                moveAction.Disable();
                lookAction.Disable();
                jumpAction.Disable();
                sprintAction.Disable();
                crouchAction.Disable();

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void Update()
    {
        // ALL players (even non-owners) execute this so they visually see others crouching
        HandleCrouchVisuals();

        // Only the owner controls movement and inputs
        if (!IsOwner) return;

        CheckGrounded();
        HandleLook();
        HandleMovement();
        HandleJump();
        HandleCrouchInput();
    }

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    // Visualize the ground check sphere in the editor for easier debugging
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    private void HandleLook()
    {
        Vector2 lookInput = lookAction.ReadValue<Vector2>() * mouseSensitivity * 0.1f;

        // Up/Down looking (Pitch) - Rotate the camera object
        xRotation -= lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Stop the camera from doing backflips
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Left/Right looking (Yaw) - Rotate the whole player body
        transform.Rotate(Vector3.up * lookInput.x);
    }

    private void HandleMovement()
    {
        // Determine current speed based on input states
        float currentSpeed = baseSpeed;
        if (isCrouched.Value) currentSpeed = crouchSpeedMult * baseSpeed;
        else if (sprintAction.IsPressed()) currentSpeed = sprintSpeedMult * baseSpeed;

        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = transform.right * input.x + transform.forward * input.y;
        moveDirection = moveDirection.normalized * currentSpeed;

        // Apply to rigidbody (preserving the Y velocity for gravity and jumping)
        rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
    }

    private void HandleJump()
    {
        // WasPressedThisFrame ensures they have to tap it, not just hold it down
        if (jumpAction.WasPressedThisFrame())
        {
            Debug.Log($"[PlayerMovement] jumpAction.WasPressedThisFrame: true, isGrounded: {isGrounded}");
        }
        if (jumpAction.WasPressedThisFrame() && isGrounded)
        {
            Debug.Log($"[PlayerMovement] jumpAction.WasPressedThisFrame: true, isGrounded: {isGrounded}");

            // Reset Y velocity first so downward momentum doesn't eat the jump
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void HandleCrouchInput()
    {
        bool wantsToCrouch = crouchAction.IsPressed();

        // Only update the network variable if the state actually changes (saves bandwidth)
        if (isCrouched.Value != wantsToCrouch)
        {
            isCrouched.Value = wantsToCrouch;
        }
    }

    private void HandleCrouchVisuals()
    {
        // This physically shrinks the capsule collider and moves the camera down
        if (isCrouched.Value)
        {
            capsuleCollider.height = crouchHeight;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraCrouchPos, Time.deltaTime * crouchAnimSpeed);
        }
        else
        {
            capsuleCollider.height = standingHeight;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, cameraStandingPos, Time.deltaTime * crouchAnimSpeed);
        }
    }


    private void OnCollisionStay(Collision collision)
    {
        if (!IsOwner || IsServer) return;
        SharedPhysicsObject pushableObject = collision.gameObject.GetComponent<SharedPhysicsObject>();
        if (pushableObject != null)
        {
            Vector3 pushDirection = -collision.contacts[0].normal;
            pushDirection.Normalize();
            Vector3 force = pushDirection * clientPushForce;
            // Apply the force to the object on the server
            pushableObject.ApplyPushForceServerRPC(force, collision.contacts[0].point);
        }
    }
}