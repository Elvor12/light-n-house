using UnityEngine;

public class Monster : MonoBehaviour
{
    [SerializeField] private float runSpeed = 5f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private string shadowZoneTag = "ShadowZone";
    [SerializeField] private float shadowStayTime = 3f;

    public Transform lighthouse;

    public delegate void MonsterDestroyed();
    public event MonsterDestroyed OnMonsterDestroyed;

    private Rigidbody2D rb;
    private bool escaping = false;
    private bool inShadow = false;

    private Vector2 runDirection;
    private Vector2 escapeTarget;
    private Vector2 returnTarget;

    private Camera mainCamera;

    private float shadowTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;

        if (lighthouse != null)
        {
            StartMovingToLighthouse();
        }
    }

    void StartMovingToLighthouse()
    {
        inShadow = false;
        escaping = false;
        returnTarget = lighthouse.position;

        Vector2 position = rb.position;
        Vector2 desiredDir = ((Vector2)returnTarget - position).normalized;
        Vector2 newDir = TryFindEscapeDirection(position, desiredDir);

        if (newDir != Vector2.zero)
        {
            runDirection = newDir;
            inShadow = false;
            escaping = false;
        }
        else
        {
            runDirection = Vector2.zero;
        }
    }

    Vector2 TryFindEscapeDirection(Vector2 currentPos, Vector2 desiredDir)
    {
        float maxAngle = 90f;
        float angleStep = 5f;

        for (float angle = 0; angle <= maxAngle; angle += angleStep)
        {
            Vector2 testDir1 = Quaternion.Euler(0, 0, angle) * desiredDir;
            if (!Physics2D.Raycast(currentPos, testDir1, runSpeed * Time.fixedDeltaTime + 0.1f, obstacleMask))
                return testDir1.normalized;

            Vector2 testDir2 = Quaternion.Euler(0, 0, -angle) * desiredDir;
            if (!Physics2D.Raycast(currentPos, testDir2, runSpeed * Time.fixedDeltaTime + 0.1f, obstacleMask))
                return testDir2.normalized;
        }
        return Vector2.zero;
    }

    void FixedUpdate()
    {
        Vector2 position = rb.position;

        if (escaping && lighthouse != null)
        {
            Vector2 desiredDir = (escapeTarget - position).normalized;
            Vector2 newDir = TryFindEscapeDirection(position, desiredDir);

            if (newDir == Vector2.zero)
                return;

            runDirection = newDir;
            Vector2 nextPos = position + runDirection * runSpeed * Time.fixedDeltaTime;
            rb.MovePosition(nextPos);

            float distToTarget = Vector2.Distance(nextPos, escapeTarget);
            if (distToTarget < 0.1f || IsOutOfCameraView(nextPos))
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
                    gameObject.SetActive(false);
                }
            }
        }
        else if (inShadow && lighthouse != null)
        {
            shadowTimer += Time.fixedDeltaTime;
            if (shadowTimer >= shadowStayTime)
            {
                Vector2 desiredDir = ((Vector2)returnTarget - position).normalized;
                Vector2 newDir = TryFindEscapeDirection(position, desiredDir);

                if (newDir == Vector2.zero)
                    return;

                runDirection = newDir;
                Vector2 nextPos = position + runDirection * runSpeed * Time.fixedDeltaTime;
                rb.MovePosition(nextPos);

                float distToReturn = Vector2.Distance(nextPos, returnTarget);
                if (distToReturn < 0.1f)
                {
                    inShadow = false;
                    Debug.Log($"{gameObject.name} добежал до маяка!");
                }
            }
        }
        else
        {
            if (lighthouse != null)
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
                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }

        public void EscapeFrom(Transform lighthouseCenter)
    {
        if (!escaping && !inShadow)
        {
            lighthouse = lighthouseCenter;

            Vector2 dirToLighthouse = (lighthouse.position - transform.position).normalized;
            float distToLighthouse = Vector2.Distance(lighthouse.position, transform.position);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToLighthouse, distToLighthouse, obstacleMask);

            if (hit.collider == null)
            {
                Vector3 offScreenPoint = GetClosestPointOutsideScreen(transform.position);
                Collider2D shadowZone = FindClosestShadowZone(transform.position);
                Vector3 shadowPoint = shadowZone != null ? shadowZone.ClosestPoint(transform.position) : transform.position;

                float distToShadow = Vector3.Distance(transform.position, shadowPoint);
                float distToOffScreen = Vector3.Distance(transform.position, offScreenPoint);

                if (distToShadow < distToOffScreen)
                    escapeTarget = shadowPoint;
                else
                    escapeTarget = offScreenPoint;

                runDirection = ((Vector2)escapeTarget - rb.position).normalized;
                escaping = true;
            }
            else
            {
                Debug.Log($"{gameObject.name} спрятался в тени!");
            }
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

    Vector3 GetClosestPointOutsideScreen(Vector3 pos)
    {
        Vector3 screenPos = mainCamera.WorldToViewportPoint(pos);

        float leftDist = screenPos.x;
        float rightDist = 1f - screenPos.x;
        float bottomDist = screenPos.y;
        float topDist = 1f - screenPos.y;

        float minDist = Mathf.Min(leftDist, rightDist, bottomDist, topDist);

        Vector3 targetViewportPos = screenPos;

        if (minDist == leftDist)
            targetViewportPos.x = -0.1f;
        else if (minDist == rightDist)
            targetViewportPos.x = 1.1f;
        else if (minDist == bottomDist)
            targetViewportPos.y = -0.1f;
        else
            targetViewportPos.y = 1.1f;

        targetViewportPos.z = screenPos.z;

        return mainCamera.ViewportToWorldPoint(targetViewportPos);
    }

    bool IsOutOfCameraView(Vector3 pos)
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(pos);
        return viewportPos.x < 0f || viewportPos.x > 1f || viewportPos.y < 0f || viewportPos.y > 1f;
    }

    void OnDisable()
    {
        OnMonsterDestroyed?.Invoke();
    }
}
