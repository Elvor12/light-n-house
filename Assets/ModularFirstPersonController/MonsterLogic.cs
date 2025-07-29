using System.IO;
using Unity.VisualScripting;
using UnityEditor;
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
    private bool isLooking = true;
    private bool fixedLook = false;

    private bool goingToMid = false;
    private Vector3 targetLastTimeSeenPos = Vector3.zero;
    private Vector3 smootheLookDirection = Vector3.zero;
    private Vector3 fixedLookDirection = Vector3.zero;

    public LayerMask obstacleMask;

    public LayerMask playerMask;

    public float raycastAngle = 45;
    public float lookLenght = 5;



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
        isLooking = false;
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
            if (agent.CalculatePath(targetPos.position, path) && path.status == NavMeshPathStatus.PathComplete && ObserveCheck())
            {
                if (Vector3.Distance(agent.destination, targetPos.position) > 1f)
                {
                    agent.path = path;
                    agent.isStopped = false;
                }
                
                settedTarget = targetPos;
                targetFollowed = true;
                targetLastTimeSeen = false;
            }
            else
            {
                if (settedTarget == targetPos)
                {
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
                NavMeshPath path = new NavMeshPath();
                Vector3 wanderTarget = ValidNavMeshPoint(transform.position, wanderDist, -1, maxDistForTarget, minDistForTarget, agent);
                if (wanderTarget != transform.position && agent.CalculatePath(wanderTarget, path))
                {
                    agent.path = path;
                    agent.isStopped = true;
                }
            }
            if (isLooking && agent.isStopped)
            {
                agent.isStopped = false;
                timer = setWanderTimer;
            }
        }
        else
        {
            timer = setWanderTimer;
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
                Vector3 midDirection = CalculateMidDirection(smootheLookDirection, desiredLookDirection);
                smootheLookDirection = UpdateSmootheLookDirection(smootheLookDirection, midDirection, desiredLookDirection);

                Quaternion targetRotation = Quaternion.LookRotation(smootheLookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, viewTimer * Time.deltaTime);
                float angleDifference = Vector3.Angle(desiredLookDirection, smootheLookDirection);
                
                if (agent.isStopped)
                {
                    isLooking = angleDifference < 5f;
                    Debug.Log(Vector3.Angle(desiredLookDirection, smootheLookDirection));
                }
                
            }
            else
            {
                Debug.Log("yep");
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
                    fixedLook = false;
                }

                if (!fixedLook)
                {
                    float k = 0.2f;
                    float maxHeightOffset = .7f;

                    float heightOffset = Mathf.Min(distToNode * k, maxHeightOffset);
                    Vector3 targetPointWithOffset = nextNode + Vector3.up * heightOffset;
                    if (Vector3.Angle((targetPointWithOffset - currentPos).normalized, Vector3.down) > 45f)
                    {
                        fixedLookDirection = (targetPointWithOffset - currentPos).normalized;
                        fixedLook = true;
                    }
                    
                }
                break;
            }
        }

        return currentPos + fixedLookDirection * directionLenght;
    }

    private Vector3 CalculateMidDirection(Vector3 currentDir, Vector3 endDirection)
    {
        Vector3 down = Vector3.down;
        float deadZoneAngle = 70f;
        currentDir.Normalize();
        endDirection.Normalize();
        float offsetStep = 0.1f;

        Vector3 rotationAxis = Vector3.Cross(currentDir, endDirection);
        if (rotationAxis == Vector3.zero)
        {
            return endDirection;
        }

        rotationAxis.Normalize();

        Vector3 midDirection = Vector3.Slerp(currentDir, endDirection, 0.5f).normalized;

        if (Vector3.Angle(midDirection, down) >= deadZoneAngle)
        {
            return midDirection;
        }
        
        for (int i = 1; i < 100; i++)
        {
            float offset = offsetStep * i;

            Vector3 directionPos = (midDirection + rotationAxis * offset).normalized;
            if (Vector3.Angle(directionPos, down) >= deadZoneAngle)
            {
                Debug.Log(midDirection);
                Debug.Log(rotationAxis);

                goingToMid = true;
                return directionPos;
            }
            Vector3 directionNeg = (midDirection - rotationAxis * offset).normalized;
            if (Vector3.Angle(directionNeg, down) >= deadZoneAngle)
            {
                Debug.Log(midDirection);
                Debug.Log(rotationAxis);
                goingToMid = true;
                return directionNeg;
            }
        }
        Debug.Log("Skill issue");
        goingToMid = true;
        return midDirection;
    }

    private Vector3 UpdateSmootheLookDirection(Vector3 startDirection, Vector3 midDirection, Vector3 endDirection)
    {
        startDirection.Normalize();
        midDirection.Normalize();
        endDirection.Normalize();

        float angleStartEnd = Vector3.Angle(startDirection, endDirection);
        float angleStartMid = Vector3.Angle(startDirection, midDirection);
        float angleMidEnd = Vector3.Angle(midDirection, endDirection);

        if (goingToMid && Mathf.Abs(angleStartEnd - (angleStartMid + angleMidEnd)) < 0.01f)
        {
            goingToMid = false;
        }

        float proportion = goingToMid ? Mathf.Clamp01(angleMidEnd / angleStartEnd) : 1f;

        float interpolationFactor = proportion * Time.deltaTime * 3f;

        if (goingToMid)
            return Vector3.Slerp(startDirection, midDirection, interpolationFactor);
        else
            return Vector3.Slerp(startDirection, endDirection, interpolationFactor);
    }

    private bool ObserveCheck()
    {
        Vector3 directionToTarget = targetPos.position - transform.position;
        float angle = Vector3.Angle(directionToTarget, smootheLookDirection);

        if (angle < raycastAngle)
        {
            Ray ray = new Ray(transform.position, directionToTarget.normalized);

            if (Physics.Raycast(ray, out RaycastHit hit, lookLenght) && (1 << hit.transform.gameObject.layer) == playerMask)
            {
                targetLastTimeSeenPos = targetPos.position;
                return true;
            }
        }
        return false;
    }

    private void NodeFixator()
    {
        if (agent.hasPath && agent.path.corners.Length > 1 && currentPathCornerIndex < agent.path.corners.Length)
        {
            Vector3 nextCorner = agent.path.corners[currentPathCornerIndex];
            float distToNextNode = Vector3.Distance(transform.position, nextCorner);

            if (distToNextNode < 1f)
            {
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
 
/*         if (targetPos == null)
            return;

        Gizmos.color = Color.green;
        Vector3 origin = transform.position;

        Vector3 direction = smootheLookDirection.normalized;
        Gizmos.DrawRay(origin, direction * lookLenght);
        
        float halfAngle = raycastAngle * 0.5f;

        int segments = 10;
        Vector3 up = Vector3.up;

        if (Vector3.Dot(direction, up) > 0.9f) // если взгляд почти вверх, берем другой вектор
            up = Vector3.forward;

        Vector3 right = Vector3.Cross(direction, up).normalized;

        up = Vector3.Cross(right, direction).normalized;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -halfAngle + i * (raycastAngle / segments);
            // Поворачиваем направление взгляда на angle вокруг оси 'up'
            Quaternion rotation = Quaternion.AngleAxis(angle, up);
            Vector3 rayDir = rotation * direction;
            Gizmos.DrawRay(origin, rayDir * lookLenght);
        } 
     */

    }
}
