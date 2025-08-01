using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour
{
    public bool isLocked = true;
    public float openAngle = 90f;
    public float duration = 1f;

    private bool isOpen = false;
    private bool isMoving = false;

    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, openAngle, 0) * closedRotation;
    }

    public void Unlock()
    {
        Debug.Log("Дверь разблокирована!");
        ToggleDoor();
    }
    public void ToggleDoor()
    {
        if (!isMoving)
        {
            StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation, isOpen ? closedRotation : openRotation));
            isOpen = !isOpen;
        }
    }

    IEnumerator RotateDoor(Quaternion fromRotation, Quaternion toRotation)
    {
        isMoving = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(fromRotation, toRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = toRotation;
        isMoving = false;
    }
}
