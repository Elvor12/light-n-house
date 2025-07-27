using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MonsterLogic : MonoBehaviour
{

    public GameObject mainTarget;
    public float wanderDist = 10f;
    public float setWanderTimer = 100f;
    public float directionLenght = 5f;
    public LineRenderer linerender;
    public float lineLenght;

    public float maxDistForTarget = 5f;
    public float minDistForTarget = 2f;
    public float minDistForDirection = 3f;
    public float timer;
    public float viewTimer = 2f;
    private Transform targetPos;
    private NavMeshAgent agent;

    private int currentPathCornerIndex = 1;

    private Transform settedTarget = null;
    private bool targetFollowed = false;
    private bool targetLastTimeSeen = false;
    private bool isLooking = false;
    private bool fixedLook = false;
    private Vector3 targetLastTimeSeenPos = Vector3.zero;
    private Vector3 smootheLookDirection = Vector3.zero;
    private Vector3 fixedLookDirection = Vector3.zero;

    public LayerMask obstacleMask;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPos = mainTarget.GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        timer = setWanderTimer;
    }

    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    agent.SetDestination(targetPos.position);
    //}

    void Update()
    {
        NodeFixator();
        UpdateViewDirection();

        UpdateChaseSetup();
        UpdateWanderSetup();
        
        
        
        SetLine();
    }

    private void UpdateChaseSetup()
    {
        if (mainTarget != null)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPos.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                if (!agent.hasPath || Vector3.Distance(agent.destination, targetPos.position) > 1f) SetDestination(targetPos.position);

                settedTarget = targetPos;
                targetFollowed = true;
                targetLastTimeSeen = false;
            }
            else
            {
                if (settedTarget == targetPos)
                {
                    targetLastTimeSeenPos = targetPos.position;
                    SetDestination(targetLastTimeSeenPos);
                    targetLastTimeSeen = true;
                }
                settedTarget = null;
                targetFollowed = false;
            }
        }
        else
        {
            settedTarget = null;
            targetFollowed = false;
            targetLastTimeSeen = false;
        }
    }

    private void UpdateWanderSetup()
    {
        if (settedTarget == null || !targetFollowed)
        {
            timer -= Time.deltaTime;

            if (timer <= 0 && !agent.hasPath)
            {
                Vector3 wanderTarget = ValidNavMeshPoint(transform.position, wanderDist, -1, maxDistForTarget, minDistForTarget, agent);
                if (wanderTarget != transform.position)
                {
                    SetDestination(wanderTarget);
                }
                timer = setWanderTimer;
            }
        }
    }

    private void UpdateViewDirection()
    {
        Vector3 desiredLookDirection;
        Vector3 currentPos = transform.position;
        if (agent.hasPath)
        {
            if (settedTarget != null && targetFollowed)
            {
                desiredLookDirection = settedTarget.position - currentPos;
            }
            else if (targetLastTimeSeen)
            {
                desiredLookDirection = targetLastTimeSeenPos - currentPos;

                targetLastTimeSeen = false;
            }
            else
            {
                desiredLookDirection = AbstractDirection() - currentPos;
            }

            if (desiredLookDirection.sqrMagnitude > 0.5f)
            {
                desiredLookDirection.Normalize();

                smootheLookDirection = Vector3.Slerp(smootheLookDirection, desiredLookDirection, viewTimer * Time.deltaTime);

                Quaternion targetRotation = Quaternion.LookRotation(smootheLookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, viewTimer * Time.deltaTime);

                float angleDifference = Quaternion.Angle(targetRotation, transform.rotation);
                isLooking = angleDifference < 10f;
            }
            else
            {
                isLooking = true;
            }
        }
        else
        {
            fixedLook = false;
            currentPathCornerIndex = 1;
        }
        
    }

    private Vector3 AbstractDirection()
    {
        Vector3 currentPos = transform.position;
        
        if (!agent.hasPath || agent.path.corners.Length == 0) return currentPos + transform.forward * directionLenght;

        for (int i = currentPathCornerIndex; i < agent.path.corners.Length; i++)
        {

            Vector3 nextNode = agent.path.corners[i];
            float distToNode = Vector3.Distance(currentPos, nextNode);

            if (distToNode > minDistForDirection && IsVisible(currentPos, nextNode))
            {

                if (currentPathCornerIndex != i)
                {
                    Debug.Log("changing index");
                    fixedLook = false;
                }

                if (!fixedLook)
                {
                    float k = 0.4f;
                    float maxHeightOffset = 1f;

                    float heightOffset = Mathf.Min(distToNode * k, maxHeightOffset);
                    Vector3 targetPointWithOffset = nextNode + Vector3.up * heightOffset;
                    fixedLookDirection = (targetPointWithOffset - currentPos).normalized;
                    fixedLook = true;
                }
                break;
            }
        }

        return currentPos + fixedLookDirection * directionLenght;

    }

    private void NodeFixator()
    {
        if (agent.hasPath && agent.path.corners.Length > 1 && currentPathCornerIndex < agent.path.corners.Length)
        {
            Vector3 nextCorner = agent.path.corners[currentPathCornerIndex];
            float distToNextNode = Vector3.Distance(transform.position, nextCorner);

            if (distToNextNode < 1f)
            {
                Debug.Log("++");
                currentPathCornerIndex++;
                fixedLook = false;
            }
        }
        else
        {
            fixedLook = false;
            currentPathCornerIndex = 1;
        }
    }

    private void SetDestination(Vector3 target)
    {
        agent.SetDestination(target);
        fixedLook = false;
        currentPathCornerIndex = 1;
    }
    private static Vector3 ValidNavMeshPoint(Vector3 originPos, float dist, int mask, float maxPathLenght, float minDist, NavMeshAgent agent)
    {
        int tries = 10;
        for (int i = 0; i <= tries; i++)
        {
            Vector3 point = NavMeshPoint(originPos, dist, mask);

            if (Vector3.Distance(originPos, point) < minDist) continue;

            NavMeshPath path = new();
            if (agent.CalculatePath(point, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                float lenght = GetPathLenght(path);

                if (lenght <= maxPathLenght && lenght > minDist)
                {
                    return point;
                }
            }
        }


        return originPos;
    }

    private static Vector3 NavMeshPoint(Vector3 originPos, float dist, int mask)
    {
        Vector3 randomPos = Random.insideUnitSphere * dist;
        randomPos += originPos;

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, dist, mask))
        {
            return hit.position;
        }
        return originPos;
    }

    private static float GetPathLenght(NavMeshPath path)
    {
        float lenght = 0f;
        if (path.corners.Length < 2)
        {
            return lenght;
        }
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            lenght += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        return lenght;
    }
    private Vector3 GetNextNode() 
    {
        NavMeshPath path = agent.path;
        Vector3 currentPos = transform.position;
        if(agent.path != null && path.corners.Length > 1) 
        {
            return path.corners[1];
        }
        return path.corners[0];
    }
    private void SetLine() 
    {
        Vector3 startPos = transform.position;
        Vector3 secondPos = startPos + smootheLookDirection.normalized * lineLenght;
        linerender.positionCount = 2;
        linerender.SetPosition(0, startPos);
        linerender.SetPosition(1, secondPos);
    }

    private bool IsVisible(Vector3 from, Vector3 to)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        dir.Normalize();
        return !Physics.Raycast(from, dir, dist, obstacleMask);
    }

    private void OnDrawGizmos()
    {
        if (agent == null || agent.path == null || agent.path.corners == null)
            return;

        Gizmos.color = Color.green;

        Vector3[] corners = agent.path.corners;
        for (int i = 0; i < corners.Length; i++)
        {
            Gizmos.DrawSphere(corners[i], 0.2f);

            if (i < corners.Length - 1)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }

        if (currentPathCornerIndex < corners.Length)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(corners[currentPathCornerIndex], 0.3f);
        }
    }
}
