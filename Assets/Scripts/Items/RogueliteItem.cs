using UnityEngine;

[CreateAssetMenu(fileName = "RogueliteItem", menuName = "Roguelite/Item", order = 1)]
public class RogueliteItem : ScriptableObject
{
    [Tooltip("Unique id for networking (must be unique across all items)")]
    public int itemId;

    public string itemName;
    [TextArea(3, 8)]
    public string description;

    [Header("Visuals")]
    public Sprite icon;
    [Tooltip("Prefab to instantiate on the world pickup (can be simple mesh) - optional")]
    public GameObject worldPrefab;
    [Tooltip("Model prefab that will be attached to the player when equipped")]
    public GameObject modelPrefab;

    [Tooltip("Slot label used for attachment routing (examples: Necklace, Finger, Trinket)")]
    public string slotTag = "General";

    [Tooltip("Local position override applied when this item is attached")]
    public Vector3 modelLocalPosition = Vector3.zero;
    [Tooltip("Local euler rotation override applied when this item is attached")]
    public Vector3 modelLocalEuler = Vector3.zero;
    [Tooltip("Local scale override applied when this item is attached")]
    public Vector3 modelLocalScale = Vector3.one;

    [Header("Gameplay")]
    public bool isConsumable = false;
}
