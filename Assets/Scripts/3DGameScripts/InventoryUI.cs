using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Image fuseImage;
    public Text inhalerCountText;
    public Inventory inventory;

    void Update()
    {
        int count = inventory.GetItemCount("Inhaler");
        inhalerCountText.text = count.ToString();

        int countInhaler = inventory.GetItemCount("Fuse");
        if (countInhaler != 0)
        {
            fuseImage.enabled = true;
        }
        else
        {
            fuseImage.enabled = false;
        }
    }
}
