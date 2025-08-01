using UnityEngine;

[CreateAssetMenu(fileName = "InhalerItem", menuName = "Items/Inhalers")]
public class Inhaler : Item
{
    public float speed = 0.5f;
    private FirstPersonController player;
    public override void Use(GameObject target, Inventory inventory)
    {

        player = FindAnyObjectByType<FirstPersonController>();
        if (player == null)
        {
            Debug.LogWarning("FirstPersonController not found!");
            return;
        }
        player.RegenerateSprint(speed);
        inventory.RemoveItem(this);
        Debug.Log("Use inhaler");

    }
}
