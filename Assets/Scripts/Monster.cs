using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask lightMask;
    [SerializeField] private string shadowZoneTag = "ShadowZone";
    [SerializeField] private float shadowStayTime = 3f;
    [SerializeField] private float escapeRadius = 15f;

    public Transform lighthouse;

    public delegate void MonsterDestroyed();
    public event MonsterDestroyed OnMonsterDestroyed;

    private Rigidbody2D rb;
    private bool escaping = false;
    private bool inShadow = false;

    private Vector2 runDirection;
    private Vector2 escapeTarget;
    private Vector2 returnTarget;

    private float shadowTimer = 0f;
    private int panicSide = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (lighthouse != null) StartMovingToLighthouse();
    }

    void StartMovingToLighthouse()
    {
        inShadow = false;
        escaping = false;
        returnTarget = lighthouse.position;

        Vector2 position = rb.position;
        Vector2 desiredDir = ((Vector2)returnTarget - position).normalized;
        Vector2 newDir = TryFindEscapeDirection(position, desiredDir);

        runDirection = newDir != Vector2.zero ? newDir : Vector2.zero;
    }

    void FixedUpdate()
    {
        Vector2 position = rb.position;

        if (!escaping && !inShadow)
        {
            Collider2D lightHit = Physics2D.OverlapPoint(position, lightMask);
            if (lightHit != null)
            {
                EscapeFrom(lighthouse);
                return;
            }
        }

        if (escaping && lighthouse != null)
        {
            UpdateEscapeTarget();
            Vector2 desiredDir = (escapeTarget - position).normalized;
            Vector2 newDir = TryFindEscapeDirection(position, desiredDir);
            runDirection = newDir;

            Vector2 nextPos = position + runDirection * runSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            float distToTarget = Vector2.Distance(nextPos, escapeTarget);
            if (distToTarget < 0.1f || Vector2.Distance(nextPos, lighthouse.position) > escapeRadius)
            {
                if (Vector2.Distance(escapeTarget, GetClosestPointInShadowZone(nextPos)) < 0.1f)
                {
                    escaping = false;
                    inShadow = true;
                    shadowTimer = 0f;
                    returnTarget = lighthouse.position;
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            Debug.DrawRay(rb.position, runDirection * 1.5f, Color.red);
        }
        else if (inShadow && lighthouse != null)
        {
            shadowTimer += Time.fixedDeltaTime;
            if (shadowTimer >= shadowStayTime)
            {
                StartMovingToLighthouse();
            }
        }
        else if (lighthouse != null)
        {
            Vector2 desiredDir = ((Vector2)returnTarget - position).normalized;
            Vector2 newDir = TryFindEscapeDirection(position, desiredDir);
            if (newDir != Vector2.zero)
            {
                runDirection = newDir;
                Vector2 nextPos = position + runDirection * runSpeed * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);

                float distToReturn = Vector2.Distance(nextPos, returnTarget);
                if (distToReturn < 0.1f)
                {
                    Debug.Log($"{gameObject.name} добежал до маяка!");
                    Destroy(gameObject);
                }
            }
        }
    }

    Vector2 TryFindEscapeDirection(Vector2 currentPos, Vector2 desiredDir)
    {
        float maxAngle = 90f;
        float angleStep = 5f;
        float checkDistance = runSpeed * Time.fixedDeltaTime + 0.1f;


        for (float angle = 0; angle <= maxAngle; angle += angleStep)
        {
            Vector2[] testDirs = new Vector2[]
            {
                Quaternion.Euler(0, 0, angle) * desiredDir,
                Quaternion.Euler(0, 0, -angle) * desiredDir
            };

            foreach (var testDir in testDirs)
            {
                bool hitsObstacle = Physics2D.Raycast(currentPos, testDir, checkDistance, obstacleMask);
                bool hitsLight = Physics2D.Raycast(currentPos, testDir, checkDistance, lightMask);

                if (!hitsObstacle && !hitsLight)
                    return testDir.normalized;

            }
        }



        if (lighthouse != null)
        {
            Vector2 awayDir = (currentPos - (Vector2)lighthouse.position).normalized;
            Vector2 sideStepDir = (panicSide > 0)
                ? new Vector2(awayDir.y, -awayDir.x)
                : new Vector2(-awayDir.y, awayDir.x);

            return sideStepDir.normalized;
        }

        return Vector2.down;
    }

    void UpdateEscapeTarget()
    {
        Vector2 position = rb.position;

        Collider2D shadowZone = FindClosestShadowZone(position);
        Vector3 shadowPoint = shadowZone != null ? shadowZone.ClosestPoint(position) : position;

        Vector2 dirAway = (position - (Vector2)lighthouse.position).normalized;
        Vector3 radiusPoint = (Vector2)lighthouse.position + dirAway * escapeRadius;

        bool shadowSafe = !Physics2D.Linecast(position, shadowPoint, lightMask);
        bool radiusSafe = !Physics2D.Linecast(position, radiusPoint, lightMask);

        if (shadowSafe && !radiusSafe)
        {
            escapeTarget = shadowPoint;
        }
        else if (!shadowSafe && radiusSafe)
        {
            escapeTarget = radiusPoint;
        }
        else if (shadowSafe && radiusSafe)
        {
            escapeTarget = (Vector3.Distance(position, shadowPoint) < Vector3.Distance(position, radiusPoint))
                ? shadowPoint
                : radiusPoint;
        }
        else
        {

            escapeTarget = position + dirAway * 2f;
        }
    }

    public void EscapeFrom(Transform lighthouseCenter)
    {
        if (!escaping && !inShadow)
        {
            lighthouse = lighthouseCenter;
            escaping = true;


            panicSide = Random.value > 0.5f ? 1 : -1;
        }
    }

    Collider2D FindClosestShadowZone(Vector3 position)
    {
        GameObject[] shadows = GameObject.FindGameObjectsWithTag(shadowZoneTag);
        Collider2D closestShadow = null;
        float minDist = Mathf.Infinity;

        foreach (var go in shadows)
        {
            Collider2D col = go.GetComponent<Collider2D>();
            if (col == null) continue;

            Vector2 closestPoint = col.ClosestPoint(position);
            float dist = Vector2.Distance(position, closestPoint);

            if (dist < minDist)
            {
                minDist = dist;
                closestShadow = col;
            }
        }
        return closestShadow;
    }

    Vector3 GetClosestPointInShadowZone(Vector3 position)
    {
        Collider2D shadowZone = FindClosestShadowZone(position);
        if (shadowZone == null) return position;
        return shadowZone.ClosestPoint(position);
    }

    void OnDisable()
    {
        OnMonsterDestroyed?.Invoke();
    }
}
