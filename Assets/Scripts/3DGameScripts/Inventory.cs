using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class Inventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();

    public void AddItem(Item item)
    {
        items.Add(item);
        Debug.Log("Добавлен предмет: " + item.itemName);
    }

    public void RemoveItem(Item item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            Debug.Log($"Удалён предмет: {item.itemName}");
        }
    }

    public bool HasItem(string item)
    {
        foreach (Item item2 in items)
        {
            if (item2.itemName == item)
            {
                return true;
            }
        }
        return false;
    }

    public int GetItemCount(string itemName)
    {
        int count = 0;
        foreach (var item in items)
        {
            if (item.itemName == itemName)
            {
                count++;
            }
        }
        return count;
    }
}
