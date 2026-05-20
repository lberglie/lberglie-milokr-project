using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Raycast-based interact system. Allows players to look at objects and press interact to use them.
/// Works for items, NPCs, buttons, and any other IInteractable-implementing objects.
/// Attach this to the player and wire it to the camera/view direction.
/// </summary>
public class InteractSystem : MonoBehaviour
{
    [Header("Raycast Settings")]
    [Tooltip("Maximum distance to raycast for interactables")]
    public float interactRange = 5f;
    [Tooltip("Layers that can be interacted with")]
    public LayerMask interactLayers = -1;

    [Header("Input")]
    [Tooltip("InputAction for the interact button. If unassigned, defaults to Keyboard 'E' + Gamepad South")]
    public InputAction interactAction;

    [Header("UI")]
    [Tooltip("Canvas group or panel showing 'Press E to interact' hint")]
    public CanvasGroup interactHintUI;
    [Tooltip("Text field for interact hint (optional)")]
    public TMPro.TextMeshProUGUI interactHintText;

    private Camera playerCamera;
    private IInteractable currentInteractable;
    private bool isInteractEnabled = true;

    private void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Setup default interact action if none is assigned
        if (interactAction == null || interactAction.bindings.Count == 0)
        {
            interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
            interactAction.AddBinding("<Gamepad>/buttonSouth");
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.Enable();

        if (interactHintUI != null)
            interactHintUI.alpha = 0f;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.Disable();

        // Always clear on disable
        if (currentInteractable != null)
        {
            currentInteractable.OnInteractableExit();
            currentInteractable = null;
        }

        if (interactHintUI != null)
            interactHintUI.alpha = 0f;
    }

    private void Update()
    {
        if (!isInteractEnabled || playerCamera == null)
            return;

        PerformInteractRaycast();
        HandleInteractInput();
    }

    private void PerformInteractRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        IInteractable newInteractable = null;

        // Raycast forward to find interactables
        if (Physics.Raycast(ray, out hit, interactRange, interactLayers))
        {
            // Support colliders on child meshes while interactable script lives on parent/root.
            IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                newInteractable = interactable;
            }
        }

        // Update interactable state
        if (newInteractable != currentInteractable)
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnInteractableExit();
                HideInteractHint();
            }

            currentInteractable = newInteractable;
            if (currentInteractable != null)
            {
                currentInteractable.OnInteractableEnter();
                ShowInteractHint();
            }
        }
    }

    private void HandleInteractInput()
    {
        if (interactAction != null && interactAction.triggered && currentInteractable != null)
        {
            currentInteractable.OnInteract();
        }
    }

    private void ShowInteractHint()
    {
        if (interactHintUI != null)
        {
            interactHintUI.alpha = 1f;
        }

        if (interactHintText != null && currentInteractable != null)
        {
            interactHintText.text = $"Press E to {currentInteractable.GetInteractableName()}";
        }
    }

    private void HideInteractHint()
    {
        if (interactHintUI != null)
        {
            interactHintUI.alpha = 0f;
        }
    }

    /// <summary>
    /// Temporarily disable interact (e.g., during dialogue or menus).
    /// </summary>
    public void SetInteractEnabled(bool enabled)
    {
        isInteractEnabled = enabled;
        if (!enabled)
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnInteractableExit();
                currentInteractable = null;
            }
            HideInteractHint();
        }
    }

    /// <summary>
    /// Get the currently highlighted interactable.
    /// </summary>
    public IInteractable GetCurrentInteractable()
    {
        return currentInteractable;
    }
}
