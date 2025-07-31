using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    public void PickUp(Inventory inventory)
    {
        if (inventory != null && item != null)
        {
            inventory.AddItem(item);
            Destroy(gameObject);
        }
    }
}
