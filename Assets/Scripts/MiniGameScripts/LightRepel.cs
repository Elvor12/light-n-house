using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class LightRepel : MonoBehaviour
{
    [SerializeField] private float repelForce = 20f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float deadZone = 0.1f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null || Mouse.current == null)
            return;


        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector2 targetPos = cam.ScreenToWorldPoint(mouseScreenPos);


        Vector2 currentPos = transform.position;
        Vector2 dir = -targetPos + currentPos;
        float distance = dir.magnitude;

        if (distance < deadZone)
            return;


        RaycastHit2D hit = Physics2D.Raycast(currentPos, dir.normalized, distance, enemyLayer);
        if (hit.collider != null)
        {
            targetPos = hit.point - dir.normalized * 0.1f; 
            dir = targetPos - currentPos;
            distance = dir.magnitude; 
        }

 
        transform.position = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {

                Vector2 repelDir = (rb.position - (Vector2)transform.position).normalized;
                rb.AddForce(repelDir * repelForce);
            }
        }
    }
}