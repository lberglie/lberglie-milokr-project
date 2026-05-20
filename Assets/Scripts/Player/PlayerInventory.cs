using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class PlayerInventory : NetworkBehaviour
{
    [Serializable]
    public class AttachmentSlot
    {
        public string slotTag = "General";
        public Transform attachPoint;
        public Vector3 stackOffset = new Vector3(0f, 0.025f, 0f);
    }

    // Holds all equipped item ids. Duplicates are allowed (example: 3 necklaces).
    private NetworkList<int> equippedItemIds;

    [Header("Attachment")]
    [Tooltip("Default point for items whose slotTag has no explicit mapping")]
    public Transform defaultAttachPoint;

    [Tooltip("Map slotTag values to attach points on the player rig")]
    public List<AttachmentSlot> attachmentSlots = new List<AttachmentSlot>();

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<GameObject> currentEquippedModels = new List<GameObject>();

    public event Action OnEquippedChanged;

    private void Awake()
    {
        equippedItemIds = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        equippedItemIds.OnListChanged += OnEquippedListChanged;

        // Initial sync after spawn.
        RefreshEquippedVisuals();
        if (IsOwner) OnEquippedChanged?.Invoke();
    }

    public override void OnNetworkDespawn()
    {
        equippedItemIds.OnListChanged -= OnEquippedListChanged;
    }

    private void OnEquippedListChanged(NetworkListEvent<int> listEvent)
    {
        if (debugLogs)
        {
            Debug.Log($"[PlayerInventory] List change ({listEvent.Type}) on player {OwnerClientId}. Total equipped: {equippedItemIds.Count}");
        }

        RefreshEquippedVisuals();

        if (IsOwner)
        {
            OnEquippedChanged?.Invoke();
        }
    }

    // Server-side add for security.
    public void AddEquippedItemServer(int itemId)
    {
        if (!IsServer)
        {
            Debug.LogWarning("[PlayerInventory] AddEquippedItemServer called on non-server");
            return;
        }

        if (ItemsDatabase.Instance == null)
        {
            Debug.LogWarning("[PlayerInventory] ItemsDatabase missing while trying to equip item");
            return;
        }

        RogueliteItem item = ItemsDatabase.Instance.GetItemById(itemId);
        if (item == null)
        {
            Debug.LogWarning($"[PlayerInventory] Cannot equip invalid item id {itemId}");
            return;
        }

        equippedItemIds.Add(itemId);

        if (debugLogs)
        {
            Debug.Log($"[PlayerInventory] Equipped '{item.itemName}' (id: {itemId}) for player {OwnerClientId}. Total equipped: {equippedItemIds.Count}");
        }
    }

    public List<int> GetEquippedItemIdsSnapshot()
    {
        List<int> snapshot = new List<int>(equippedItemIds.Count);
        for (int i = 0; i < equippedItemIds.Count; i++)
        {
            snapshot.Add(equippedItemIds[i]);
        }

        return snapshot;
    }

    private void RefreshEquippedVisuals()
    {
        ClearVisuals();

        if (ItemsDatabase.Instance == null) return;

        Dictionary<string, int> slotStackCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < equippedItemIds.Count; i++)
        {
            int itemId = equippedItemIds[i];
            RogueliteItem item = ItemsDatabase.Instance.GetItemById(itemId);
            if (item == null || item.modelPrefab == null) continue;

            AttachmentSlot slot = ResolveSlot(item.slotTag);
            Transform parent = slot != null && slot.attachPoint != null
                ? slot.attachPoint
                : (defaultAttachPoint != null ? defaultAttachPoint : transform);

            string key = item.slotTag ?? "General";
            int stackIndex = 0;
            if (slotStackCounts.ContainsKey(key))
            {
                stackIndex = slotStackCounts[key];
                slotStackCounts[key] = slotStackCounts[key] + 1;
            }
            else
            {
                slotStackCounts[key] = 1;
            }

            GameObject instance = Instantiate(item.modelPrefab, parent, false);
            SanitizeEquippedModelInstance(instance);
            Vector3 offset = slot != null ? slot.stackOffset * stackIndex : Vector3.zero;
            instance.transform.localPosition = item.modelLocalPosition + offset;
            instance.transform.localRotation = Quaternion.Euler(item.modelLocalEuler);
            instance.transform.localScale = item.modelLocalScale;
            currentEquippedModels.Add(instance);
        }
    }

    // Equipped item visuals should not participate in physics/network simulation.
    private static void SanitizeEquippedModelInstance(GameObject instance)
    {
        if (instance == null) return;

        // World pickup effects should not run when the model is attached to a player.
        FloatScript[] floatScripts = instance.GetComponentsInChildren<FloatScript>(true);
        for (int i = 0; i < floatScripts.Length; i++)
        {
            floatScripts[i].enabled = false;
        }

        Rigidbody[] rigidbodies = instance.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Destroy(rigidbodies[i]);
        }

        Joint[] joints = instance.GetComponentsInChildren<Joint>(true);
        for (int i = 0; i < joints.Length; i++)
        {
            Destroy(joints[i]);
        }

        Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private AttachmentSlot ResolveSlot(string slotTag)
    {
        if (string.IsNullOrWhiteSpace(slotTag)) return null;

        for (int i = 0; i < attachmentSlots.Count; i++)
        {
            AttachmentSlot slot = attachmentSlots[i];
            if (slot != null && string.Equals(slot.slotTag, slotTag, StringComparison.OrdinalIgnoreCase))
            {
                return slot;
            }
        }

        return null;
    }

    private void ClearVisuals()
    {
        for (int i = 0; i < currentEquippedModels.Count; i++)
        {
            if (currentEquippedModels[i] != null)
            {
                Destroy(currentEquippedModels[i]);
            }
        }

        currentEquippedModels.Clear();
    }

    public override void OnDestroy()
    {
        if (equippedItemIds != null)
        {
            equippedItemIds.Dispose();
        }
        base.OnDestroy();
    }
}
