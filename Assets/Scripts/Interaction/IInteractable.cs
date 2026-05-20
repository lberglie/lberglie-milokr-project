using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with via raycast.
/// Implement this on any object that the player can interact with (items, NPCs, buttons, etc).
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Called when the player starts looking at this interactable (raycast enters).
    /// </summary>
    void OnInteractableEnter();

    /// <summary>
    /// Called when the player stops looking at this interactable (raycast exits).
    /// </summary>
    void OnInteractableExit();

    /// <summary>
    /// Called when the player presses the interact button while looking at this interactable.
    /// </summary>
    void OnInteract();

    /// <summary>
    /// Get a world-position point on this interactable for UI positioning.
    /// </summary>
    Vector3 GetInteractablePosition();

    /// <summary>
    /// Get a display name for this interactable (shown in hover UI).
    /// </summary>
    string GetInteractableName();
}
