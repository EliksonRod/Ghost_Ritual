using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    Dictionary<ItemData, InventoryItem> m_itemDictionary = new Dictionary<ItemData, InventoryItem>();
    public static Inventory Instance { get; private set; }
    public List<InventoryItem> inventory { get; private set; } = new List<InventoryItem>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public class InventoryItem
    {
        public ItemData data { get; private set; }
        public int stackSize { get; private set; }

        public InventoryItem(ItemData source)
        {
            data = source;
        }

    }

    public void AddItem(ItemData referenceData)
    {
        InventoryItem newItem = new InventoryItem(referenceData);
        inventory.Add(newItem);
        m_itemDictionary.Add(referenceData, newItem);
    }

    public void DropItem(ItemData referenceData)
    {
        if (m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            if (value.stackSize == 0)
            {
                inventory.Remove(value);
                m_itemDictionary.Remove(referenceData);
            }
        }
    }

    public InventoryItem Get(ItemData referenceData)
    {
        if (m_itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            return value;
        }
        return null;
    }

    [Serializable]
    public struct ItemRequirement
    {
        public ItemData itemData;
        public int amount;

        public bool HasRequirement()
        {
            InventoryItem item = Inventory.Instance.Get(itemData);

            if (item == null || item.stackSize < amount) { return false; }

            return true;
        }
    }
}