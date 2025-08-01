using UnityEngine;

[CreateAssetMenu(fileName = "KeyItem", menuName = "Items/Key")]
public class KeyItem : Item
{
     public override void Use(GameObject target, Inventory inventory)
    {
        Door door = target.GetComponent<Door>();
        if (door != null)
        {
            door.Unlock();
            inventory.RemoveItem(this);
        }
    }
}
