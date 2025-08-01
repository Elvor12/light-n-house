using System.Security;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float distance = 10f;
    public LayerMask triggerObj;
    public ScenesManager manager;
    public LayerMask obstacle;
    public LayerMask itemPickUp;
    public LayerMask usableItemLayer;
    private int mask;
    private FirstPersonController player;
    public Inventory inventory;

    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<FirstPersonController>();
        cam = Camera.main;
        mask = triggerObj.value | obstacle.value | itemPickUp.value | usableItemLayer.value;
    }

    public bool CheckForTrigger()
    {
        RaycastHit hit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            if ((1 << hit.collider.gameObject.layer) == triggerObj.value)
            {
                manager.SwitchToSecond();
                return true;
            }

            if ((1 << hit.collider.gameObject.layer) == itemPickUp.value)
            {
                GameObject target = hit.collider.gameObject;
                ItemPickup pickup = target.GetComponent<ItemPickup>();
                if (pickup.item.itemName == "Petard")
                {
                    pickup.Heal(player);
                    pickup.Destroy();
                    return true;
                }
                if (pickup != null)
                {
                    Inventory inventory = FindAnyObjectByType<Inventory>();
                    if (inventory != null)
                    {
                        pickup.PickUp(inventory);
                        if (pickup.item.itemName != "Fuse")
                        {
                            pickup.Destroy();
                        }
                        return true;
                    }
                }
            }
            if ((1 << hit.collider.gameObject.layer) == usableItemLayer.value)
            {
                GameObject target = hit.collider.gameObject;
                foreach (Item item in inventory.items)
                {
                    if (item != null)
                    {
                        item.Use(target, inventory);
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
