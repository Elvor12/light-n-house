using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]

public abstract class Item : ScriptableObject
{
    public string itemName;
    public abstract void Use(GameObject target, Inventory inventory);
}
