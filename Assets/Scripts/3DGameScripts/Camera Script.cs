using System.Security;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    public float distance = 10f;
    public LayerMask triggerObj;
    public ScenesManager manager;
    public LayerMask obstacle;
    public LayerMask itemPickUp;
    public LayerMask usableItemLayer;
    public LayerMask repairMinigame;
    private int mask;
    private FirstPersonController player;
    public Inventory inventory;
    public bool isMiniGameWork = true;

    public TextBlinkController blinkController;
    public TextBlinkController blinkController2;
    public Text firstText;
    public Text secondText;


    private float timerBrokeMiniGame = 0f;
    public float brokeTime = 10f;

    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<FirstPersonController>();
        cam = Camera.main;
        mask = triggerObj.value | obstacle.value | itemPickUp.value | usableItemLayer.value | repairMinigame.value;
    }


    private void Update()
    {
        timerBrokeMiniGame += Time.deltaTime;
        if (timerBrokeMiniGame > brokeTime && isMiniGameWork)
        {
            isMiniGameWork = false;
            blinkController.BlinkText(firstText);
            Debug.Log("Ã»Õ»√¿Ã≈ —ÀŒÃ¿À¿—‹");
            timerBrokeMiniGame = 0;
        }
    }

   
    public bool CheckForTrigger()
    {
        RaycastHit hit;
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out hit, distance, mask))
        {
            if ((1 << hit.collider.gameObject.layer) == triggerObj.value)
            {
                if (isMiniGameWork)
                {
                    manager.SwitchToSecond();
                    return true;
                }
                else
                {
                    blinkController2.BlinkText(secondText);
                    Debug.Log("Ã»Õ»√¿Ã≈ —ÀŒÃ¿Õ¿");
                }
            }

            if ((1 << hit.collider.gameObject.layer) == repairMinigame.value)
            {
                blinkController.StopBlink();
                blinkController2.StopBlink();
                isMiniGameWork = true;
                timerBrokeMiniGame = 0;
                Debug.Log("RepairMiniGame");
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
