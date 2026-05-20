using UnityEngine;

/// <summary>
/// Optional helper to configure equip attach point from editor.
/// Attach this to the player and assign the transform where equipped models appear.
/// </summary>
public class ItemVisualController : MonoBehaviour
{
    [Tooltip("Attach point for equipped items (e.g., necklace bone, hand)")]
    public Transform equipAttachPoint;

    private void Reset()
    {
        // try to find a reasonable default (first child named "AttachPoint")
        var t = transform.Find("AttachPoint");
        if (t != null) equipAttachPoint = t;
    }
}
