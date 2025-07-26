using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MonsterLogic : MonoBehaviour
{

    public GameObject mainTarget;
    public float wanderDist = 10f;
    public float setWanderTimer = 100f;
    public LineRenderer linerender;
    public float lineLenght;

    public float maxDist = 5;
    public float minDist = 2;
    public float timer;
    public float viewTimer = 2f;
    private Transform targetPos;
    private NavMeshAgent agent;

    private Transform settedTarget = null;
    private bool targetFollowed = false;
    private bool targetLastTimeSeen = false;
    private bool isLooking = false;
    private Vector3 targetLastTimeSeenPos = Vector3.zero;
    private Vector3 smootheLookDirection = Vector3.zero;


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
        UpdateViewDirection();
        if (isLooking)
        {
            UpdateChaseSetup();
            UpdateWanderSetup();
        }
        
        
        SetLine();
    }

    private void UpdateChaseSetup()
    {
        if (mainTarget != null)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(targetPos.position, path) && path.status == NavMeshPathStatus.PathComplete)
            {
                if (!agent.hasPath || Vector3.Distance(agent.destination, targetPos.position) > 1f) agent.SetDestination(targetPos.position);

                settedTarget = targetPos;
                targetFollowed = true;
                targetLastTimeSeen = false;
            }
            else
            {
                if (settedTarget == targetPos)
                {
                    targetLastTimeSeenPos = targetPos.position;
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
                Vector3 wanderTarget = ValidNavMeshPoint(transform.position, wanderDist, -1, maxDist, minDist, agent);
                if (wanderTarget != transform.position)
                {
                    agent.SetDestination(wanderTarget);
                }
                timer = setWanderTimer;
            }
        }
    }

    private void UpdateViewDirection()
    {
        Vector3 desiredLookDirection;

        if (settedTarget != null && targetFollowed)
        {
            desiredLookDirection = settedTarget.position - transform.position;
        }
        else if (targetLastTimeSeen)
        {
            desiredLookDirection = targetLastTimeSeenPos - transform.position;

            targetLastTimeSeen = false;
        }
        else
        {
            desiredLookDirection = GetNextDirection(smootheLookDirection);
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
    private Vector3 GetNextDirection(Vector3 defaultDir) 
    {
        NavMeshPath path = agent.path;
        Vector3 currentPos = transform.position;
        if(agent.path != null && path.corners.Length > 1) 
        {
            return path.corners[1] - currentPos;
        }
        return defaultDir;

    }
    private void SetLine() 
    {
        Vector3 startPos = transform.position;
        Vector3 secondPos = startPos + smootheLookDirection.normalized * lineLenght;
        linerender.positionCount = 2;
        linerender.SetPosition(0, startPos);
        linerender.SetPosition(1, secondPos);
    }
}
