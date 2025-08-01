using Unity.VisualScripting;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    public void PickUp(Inventory inventory)
    {
        if (inventory != null && item != null && item.itemName == "Fuse")
        {
            if (inventory.HasItem("Fuse"))
            {
                return;
            }
            else
            {
                inventory.AddItem(item);
                return;
            }

        }
        if (inventory != null && item != null)
        {
            inventory.AddItem(item);
        }
    }

    public void Heal(FirstPersonController player)
    {
        player.Heal();
    }
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
