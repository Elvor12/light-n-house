using UnityEngine;

[CreateAssetMenu(fileName = "FuseItem", menuName = "Items/Fuses")]
public class Fuse : Item
{
    public override void Use(GameObject target, Inventory inventory)
    {

        ElectricalUnit unit = target.GetComponent<ElectricalUnit>();
        if (unit != null)
        {
            unit.Repair();
            inventory.RemoveItem(this);
        }

    }
}
