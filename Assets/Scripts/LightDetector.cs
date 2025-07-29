using UnityEngine;

public class LightDetector : MonoBehaviour
{
    [SerializeField] private Transform lighthouse;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Monster monster = other.GetComponent<Monster>();
        if (monster != null)
        {
            monster.EscapeFrom(lighthouse);
        }
    }
}
