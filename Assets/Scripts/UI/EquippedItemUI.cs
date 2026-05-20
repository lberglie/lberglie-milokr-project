using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections.Generic;

/// <summary>
/// Multi-item inspect UI. Supports large equipment lists and tooltip-on-hover.
/// </summary>
public class EquippedItemUI : MonoBehaviour
{
    [Header("Inspect")]
    [Tooltip("Input Action for toggling the inspect panel. If unassigned, a default (Keyboard 'I' + Gamepad South) will be created at runtime.")]
    public InputAction inspectToggleAction;
    public GameObject inspectPanel;

    [Header("Entries")]
    [Tooltip("Parent transform (for GridLayoutGroup/HorizontalLayoutGroup/VerticalLayoutGroup)")]
    public Transform entriesRoot;
    [Tooltip("Prefab with EquippedItemEntryUI + UnityEngine.UI.Image")]
    public EquippedItemEntryUI entryPrefab;

    [Header("Tooltip")]
    public GameObject tooltipPanel;
    public TMPro.TextMeshProUGUI tooltipText;

    private PlayerInventory localInventory;
    private readonly List<EquippedItemEntryUI> activeEntries = new List<EquippedItemEntryUI>();

    private void Awake()
    {
        // Ensure panels start hidden
        if (inspectPanel != null) inspectPanel.SetActive(false);
        if (tooltipPanel != null) tooltipPanel.SetActive(false);

        // If no action assigned from inspector, create a safe default binding
        if (inspectToggleAction == null || inspectToggleAction.bindings.Count == 0)
        {
            inspectToggleAction = new InputAction("Inspect", binding: "<Keyboard>/i");
            inspectToggleAction.AddBinding("<Gamepad>/buttonSouth");
        }
    }

    private void OnEnable()
    {
        if (inspectToggleAction != null)
            inspectToggleAction.Enable();
    }

    private void Update()
    {
        if (inspectToggleAction != null && inspectToggleAction.triggered && inspectPanel != null)
        {
            bool next = !inspectPanel.activeSelf;
            inspectPanel.SetActive(next);
            if (!next && tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
    }

    private void Start()
    {
        // Wait for NetworkManager and player objects to exist
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            TryBindToLocalPlayer();
        }
        else
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        TryBindToLocalPlayer();
    }

    private void TryBindToLocalPlayer()
    {
        if (NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null) return;

        var playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        localInventory = playerObj.GetComponent<PlayerInventory>();
        if (localInventory != null)
        {
            localInventory.OnEquippedChanged += OnEquippedChanged;
            OnEquippedChanged();
        }
    }

    private void OnEquippedChanged()
    {
        ClearEntries();
        if (localInventory == null || entriesRoot == null || entryPrefab == null) return;

        List<int> itemIds = localInventory.GetEquippedItemIdsSnapshot();
        for (int i = 0; i < itemIds.Count; i++)
        {
            RogueliteItem item = ItemsDatabase.Instance != null ? ItemsDatabase.Instance.GetItemById(itemIds[i]) : null;
            if (item == null) continue;

            EquippedItemEntryUI entry = Instantiate(entryPrefab, entriesRoot);
            entry.Initialize(item, this);
            activeEntries.Add(entry);
        }

        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void ClearEntries()
    {
        for (int i = 0; i < activeEntries.Count; i++)
        {
            if (activeEntries[i] != null)
            {
                Destroy(activeEntries[i].gameObject);
            }
        }

        activeEntries.Clear();
    }

    public void ShowTooltip(RogueliteItem item)
    {
        if (item == null || tooltipPanel == null || tooltipText == null) return;
        tooltipText.text = $"<b>{item.itemName}</b>\n<i>Slot: {item.slotTag}</i>\n{item.description}";
        tooltipPanel.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    // Backward compatibility for existing EventTrigger wiring.
    public void OnPointerEnterIcon()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(true);
    }

    public void OnPointerExitIcon()
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        if (inspectToggleAction != null)
            inspectToggleAction.Disable();

        if (tooltipPanel != null) tooltipPanel.SetActive(false);

        if (localInventory != null)
        {
            localInventory.OnEquippedChanged -= OnEquippedChanged;
        }
    }
}
