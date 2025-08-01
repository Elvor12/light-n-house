using UnityEngine;

public class Door : MonoBehaviour
{
    public bool isLocked = true;

    public void Unlock()
    {
        Debug.Log("Дверь разблокирована!");
    }
}
