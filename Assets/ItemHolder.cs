using System.Collections.Generic;
using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    public static ItemHolder Instance;
    public Dictionary<string, GameObject> items = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadAllItems();
    }

    private void LoadAllItems()
    {
        GameObject[] loadedItems = Resources.LoadAll<GameObject>("Items");

        items.Clear();

        foreach (GameObject item in loadedItems)
        {
            string key = item.name;

            if (!items.ContainsKey(key))
            {
                items.Add(key, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate item name found in Resources/Items: {key}");
            }
        }

        Debug.Log($"Loaded {items.Count} items from Resources/Items.");
    }

    // Optional: quick way to get an item prefab by name
    public GameObject GetItem(string name)
    {
        if (items.TryGetValue(name, out var item))
            return item;

        Debug.LogWarning($"Item not found: {name}");
        return null;
    }

}
