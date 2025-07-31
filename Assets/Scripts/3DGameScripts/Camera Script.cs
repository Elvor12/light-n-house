using System.Security;
using UnityEditor;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public float distance = 10f;
    public LayerMask triggerObj;
    public ScenesManager manager;
    public LayerMask obstacle;
    private int mask;

    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;
        mask = triggerObj.value | obstacle.value;
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
        }
        return false;
    }
}
