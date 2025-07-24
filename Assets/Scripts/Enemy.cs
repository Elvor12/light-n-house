using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveForce = 5f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float despawnRadius = 12f;

    private Rigidbody2D rb;
    private Vector2 center = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 pos = rb.position;
        Vector2 toCenter = center - pos;
        float dist = toCenter.magnitude;


        if (dist < stopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }


        Vector2 dir = toCenter.normalized;


        rb.AddForce(dir * moveForce);


        Vector2 velocity = rb.linearVelocity;
        if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.linearVelocity = velocity.normalized * maxSpeed;
        }


        if (dist > despawnRadius)
        {
            Destroy(gameObject);
        }
    }
}
