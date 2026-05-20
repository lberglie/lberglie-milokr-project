using System.Collections.Generic;
using UnityEngine;

public class ItemsDatabase : MonoBehaviour
{
    public static ItemsDatabase Instance { get; private set; }

    [Tooltip("Populate with your RogueliteItem assets in the inspector")]
    public List<RogueliteItem> items = new List<RogueliteItem>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public RogueliteItem GetItemById(int id)
    {
        return items.Find(i => i != null && i.itemId == id);
    }
}
