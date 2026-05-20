using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : NetworkBehaviour, IInteractable
{
    [Tooltip("ID that matches a RogueliteItem in ItemsDatabase")]
    public int itemId;

    [Tooltip("Optional local visual root (non-networked) - used when worldPrefab isn't assigned at runtime")]
    public Transform visualRoot;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;
    private bool isConsumed;

    private void Reset()
    {
        // Raycast interaction does not require trigger colliders.
        Collider c = GetComponent<Collider>();
        if (c != null) c.isTrigger = false;
    }

    // IInteractable implementation - called when player looks at this item
    public void OnInteractableEnter()
    {
        if (debugLogs)
        {
            Debug.Log($"[ItemPickup] Player is looking at item id {itemId}");
        }
    }

    // IInteractable implementation - called when player stops looking at this item
    public void OnInteractableExit()
    {
        if (debugLogs)
        {
            Debug.Log($"[ItemPickup] Player stopped looking at item id {itemId}");
        }
    }

    // IInteractable implementation - called when player presses interact button
    public void OnInteract()
    {
        if (NetworkManager.Singleton == null) return;
        if (isConsumed) return;

        if (debugLogs)
        {
            Debug.Log($"[ItemPickup] Interaction key pressed for item id {itemId}. Sending pickup request.");
        }
        RequestPickupServerRPC(NetworkManager.Singleton.LocalClientId);
    }

    public Vector3 GetInteractablePosition()
    {
        return transform.position;
    }

    public string GetInteractableName()
    {
        if (ItemsDatabase.Instance != null)
        {
            var item = ItemsDatabase.Instance.GetItemById(itemId);
            if (item != null) return $"pick up {item.itemName}";
        }
        return "pick up item";
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestPickupServerRPC(ulong requesterClientId)
    {
        if (isConsumed) return;

        if (debugLogs)
        {
            Debug.Log($"[ItemPickup] Server received pickup request. requesterClientId={requesterClientId}, itemId={itemId}");
        }

        // Server validates and equips item for the player
        if (ItemsDatabase.Instance == null)
        {
            Debug.LogWarning("ItemsDatabase not found on server");
            return;
        }

        var item = ItemsDatabase.Instance.GetItemById(itemId);
        if (item == null)
        {
            Debug.LogWarning($"Invalid item id {itemId} pickup attempted");
            return;
        }

        // Find the player's PlayerInventory (player object registered in ConnectedClients)
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(requesterClientId)) return;

        var playerObj = NetworkManager.Singleton.ConnectedClients[requesterClientId].PlayerObject;
        if (playerObj == null) return;

        var inventory = playerObj.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("PlayerInventory not found on player object");
            return;
        }

        // Add equipped item on the server (network list updates and client visuals refresh)
        inventory.AddEquippedItemServer(itemId);

        if (debugLogs)
        {
            Debug.Log($"[ItemPickup] Equipped item id {itemId} for client {requesterClientId}. Despawning pickup.");
        }

        // Hide the consumed pickup on host/server and remote clients.
        SetConsumedState(true);
        SetConsumedClientRpc(true);

        // Remove pickup from the world
        var net = GetComponent<NetworkObject>();
        if (net != null && net.IsSpawned && NetworkManager.Singleton.IsServer)
        {
            net.Despawn(false);
        }
        else
        {
            // fallback for non-networked pickups
            Destroy(gameObject);
        }
    }

    [Rpc(SendTo.NotServer)]
    private void SetConsumedClientRpc(bool consumed)
    {
        SetConsumedState(consumed);
    }

    private void SetConsumedState(bool consumed)
    {
        isConsumed = consumed;

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = !consumed;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = !consumed;
        }

        if (visualRoot != null)
        {
            visualRoot.gameObject.SetActive(!consumed);
        }
    }
}
