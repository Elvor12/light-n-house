using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class MonsterLogic : MonoBehaviour
{

    public GameObject mainTarget;
    private PointsDatabase pDatabase;
    public float wanderDist = 10f;
    public float setWanderTimer = 100f;
    public float directionLenght = 5f;
    public LineRenderer linerender;
    public float lineLenght;

    public float maxDistForTarget = 5f;
    public float minDistForTarget = 2f;
    public float minDistForDirection = 3f;
    public float timer;
    private float chaseTimer;
    public float viewTimer = 2f;
    public float timerForCaution = 0;
    public float timerForShifting = 0;
    private float angleForDirectionOffset = 0;
    public int shiftingTime = 3;
    private int cautionShiftingTime = 2;
    private int rightOrLeft = 1;
    private Transform targetPos;
    private NavMeshAgent agent;

    private int currentPathCornerIndex = 1;

    private Transform settedTarget = null;
    private bool targetFollowed = false;
    private bool targetLastTimeSeen = false;
    private bool isLooking = true;
    private bool fixedLook = false;
    private bool focused = true;

    private bool goingToMid = false;
    private bool goingToMidForShifting = false;
    private bool caution = false;

    private Vector3 offsetDirection = Vector3.zero;
    private Vector3 targetLastTimeSeenPos = Vector3.zero;
    private Vector3 smootheLookDirection = Vector3.forward;
    private Vector3 shiftedLookDirection = Vector3.zero;
    private Vector3 fixedLookDirection = Vector3.zero;
    private Vector3 dynamicAxe = Vector3.up;
    public Vector3 interestPointPos = Vector3.zero;
    public Vector3 targetPointPos = Vector3.zero;
    public PatrolPoint interestPoint;

    public LayerMask obstacleMask;
    public LayerMask playerMask;

    public float raycastAngle = 45;
    public float lookLenght = 5;
    private NavMeshPath path;
    public NavMeshPath interestPath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pDatabase = FindAnyObjectByType<PointsDatabase>();
        targetPos = mainTarget.GetComponent<Transform>();
        agent = GetComponent<NavMeshAgent>();
        timer = setWanderTimer;
        path = new();
        interestPath = new();
        //interestPoint = targetPos.position;
    }

    // Update is called once per frame
    //void FixedUpdate()
    //{
    //    agent.SetDestination(targetPos.position);
    //}

    void Update()
    {
        chaseTimer += Time.deltaTime;
        isLooking = false;
        NodeFixator();
        UpdateViewDirection();

        UpdateChaseSetup();

        if (targetPointPos != Vector3.zero)
        {
            UpdateWanderSetup(targetPointPos);
        }
        else UpdateWanderSetup();

        SetLine();
        if (caution)
        {
            ActivateCautionProtocol();
        }
    }

    private void ActivateCautionProtocol()
    {
        if (timerForCaution == 0)
        {
            agent.speed = 1;
            setWanderTimer = 3;
        }
        if (!agent.isStopped) ShiftDirection(ref cautionShiftingTime, 2);
        else
        {
            cautionShiftingTime = 2;
            focused = true;
            shiftingTime = 3;
        }

        timerForCaution += Time.deltaTime;
        if (timerForCaution >= 20 && agent.isStopped || settedTarget == targetPos)
        {
            timerForCaution = 0;
            agent.speed = 3.5f;
            focused = true;
            caution = false;
        }
    }

    private void UpdateChaseSetup()
    {
        if (mainTarget != null)
        {
            if (agent.CalculatePath(targetPos.position, path) && path.status == NavMeshPathStatus.PathComplete && ObserveCheck() && chaseTimer > 0.15f)
            {
                chaseTimer = 0f;
                if (Vector3.Distance(agent.destination, targetPos.position) > 1f)
                {
                    agent.path = path;
                    agent.isStopped = false;
                }

                shiftingTime = 3;
                focused = true;
                settedTarget = targetPos;
                targetFollowed = true;
                targetLastTimeSeen = false;
            }
            else if (settedTarget == targetPos)
            {
                if (targetLastTimeSeenPos != transform.position)
                {
                    SetDestination(targetLastTimeSeenPos);
                    targetLastTimeSeen = true;
                }
                targetFollowed = false;
                if (Vector3.Distance(targetLastTimeSeenPos, transform.position) < 2f && targetLastTimeSeen && ShiftDirection(ref shiftingTime, 1))
                {
                    agent.isStopped = true;
                    shiftingTime = 3;
                    settedTarget = null;
                    caution = true;
                }
            }
        }
        else
        {
            settedTarget = null;
            targetFollowed = false;
            targetLastTimeSeen = false;
        }
    }
    public void NoiseSetup(Vector3 position)
    {
        if (agent.CalculatePath(position, path) && path.status == NavMeshPathStatus.PathComplete && position != transform.position)
        {
            agent.path = path;
            agent.isStopped = false;
        }
    }
    private void UpdateWanderSetup()
    {
        UpdateWanderSetup(Vector3.zero);
    }
    private void UpdateWanderSetup(Vector3 interest)
    {
        if (settedTarget == null && !targetFollowed)
        {

            if (interestPoint != null) targetPointPos = Vector3.zero;
            else if (targetPointPos == Vector3.zero)
            {
                targetPointPos = pDatabase.GetNewTarget();
                Debug.Log($"New target{targetPointPos}");
            }
            
            interestPath = SetPathToPoint(targetPointPos, interestPath);

            if (!agent.hasPath)
            {
                timer -= Time.deltaTime;
                if (timer <= 0)
                {
                    Vector3 wanderTarget = ValidNavMeshPoint(transform.position, wanderDist, -1, maxDistForTarget, minDistForTarget, agent, interest);
                    if (wanderTarget != transform.position && agent.CalculatePath(wanderTarget, path))
                    {
                        agent.path = path;
                        agent.isStopped = true;
                    }
                    timer = setWanderTimer;
                }
            }
            if (isLooking && agent.isStopped)
            {
                agent.isStopped = false;
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
            else
            {
                if (targetLastTimeSeen) targetLastTimeSeen = false;
                desiredLookDirection = AbstractDirection() - currentPos;
            }
            if (desiredLookDirection.sqrMagnitude > 0.5f)
            {
                desiredLookDirection.Normalize();
                Vector3 midDirection = CalculateMidDirection(smootheLookDirection, desiredLookDirection);
                smootheLookDirection = UpdateSmootheLookDirection(smootheLookDirection, midDirection, desiredLookDirection, ref goingToMid);
                Vector3 right = Vector3.Cross(smootheLookDirection, Vector3.up);
                dynamicAxe = Vector3.Cross(right, smootheLookDirection);

                Quaternion targetRotation = Quaternion.LookRotation(smootheLookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, viewTimer * Time.deltaTime);
                float angleDifference = Vector3.Angle(desiredLookDirection, smootheLookDirection);

                if (agent.isStopped)
                {
                    isLooking = angleDifference < 5f;
                }

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
        if (focused)
        {
            shiftedLookDirection = Vector3.Slerp(shiftedLookDirection, smootheLookDirection, Time.deltaTime * 5);
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

    private bool ShiftDirection(ref int shiftingTime, float board = 0f)
    {
        focused = false;

        if (shiftingTime <= 0)
        {
            goingToMidForShifting = false;
            focused = true;
            return true;
        }

        if (angleForDirectionOffset == 0)
        {
            angleForDirectionOffset = Random.Range(70, 120) * rightOrLeft;
            rightOrLeft = -rightOrLeft;
            offsetDirection = Quaternion.AngleAxis(angleForDirectionOffset, dynamicAxe) * smootheLookDirection;
        }
        shiftedLookDirection = UpdateSmootheLookDirection(shiftedLookDirection, smootheLookDirection, offsetDirection, ref goingToMidForShifting);
        if (Vector3.Angle(shiftedLookDirection, offsetDirection) < 5f && shiftingTime > 0 )
        {
            timerForShifting += Time.deltaTime;
            if (timerForShifting >= board)
            {
                goingToMidForShifting = true;
                angleForDirectionOffset = 0;
                shiftingTime -= 1;
                timerForShifting = 0;
            }
            
        }

        

        return false;
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
                goingToMid = true;
                return directionPos;
            }
            Vector3 directionNeg = (midDirection - rotationAxis * offset).normalized;
            if (Vector3.Angle(directionNeg, down) >= deadZoneAngle)
            {
                goingToMid = true;
                return directionNeg;
            }
        }
        goingToMid = true;
        return midDirection;
    }

    private Vector3 UpdateSmootheLookDirection(Vector3 startDirection, Vector3 midDirection, Vector3 endDirection, ref bool goingToMid)
    {
        startDirection.Normalize();
        midDirection.Normalize();
        endDirection.Normalize();

        float angleStartEnd = Vector3.Angle(startDirection, endDirection);
        float angleStartMid = Vector3.Angle(startDirection, midDirection);
        float angleMidEnd = Vector3.Angle(midDirection, endDirection);
        if (goingToMid && Mathf.Approximately(Mathf.Abs(angleStartEnd + 1 - (angleStartMid + angleMidEnd)), 1f))
        {
            goingToMid = false;
        }

        float proportion = goingToMid ? angleStartEnd / angleStartMid / 2 : 1f;

        float interpolationFactor = proportion * Time.deltaTime * 3f;

        if (goingToMid) {
            return Vector3.Slerp(startDirection, midDirection, interpolationFactor);
        }
        else
            return Vector3.Slerp(startDirection, endDirection, interpolationFactor);
    }

    private bool ObserveCheck()
    {
        Vector3 directionToTarget = targetPos.position - transform.position;
        float angle = Vector3.Angle(directionToTarget, shiftedLookDirection);

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
    private Vector3 ValidNavMeshPoint(Vector3 originPos, float dist, int mask, float maxPathLenght, float minDist, NavMeshAgent agent, Vector3 interest)
    {
        int tries = 10;
        for (int i = 0; i <= tries; i++)
        {
            Vector3 point = NavMeshPoint(originPos, dist, mask);

            if (Vector3.Distance(originPos, point) < minDist) continue;

            if (interest != Vector3.zero)
            {
                NavMeshPath interestPath = new();
                if (NavMesh.CalculatePath(point, interest, agent.areaMask, interestPath) && interestPath.status == NavMeshPathStatus.PathComplete)
                {
                    
                    if (GetPathLenght(interestPath) >= GetPathLenght(this.interestPath))
                    {
                        continue;
                    }
                }
                else return originPos;
            }            
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
    private NavMeshPath SetPathToPoint(Vector3 point, NavMeshPath path)
    {
        agent.CalculatePath(point, path);
        return path;
    }

    private Vector3 NavMeshPoint(Vector3 originPos, float dist, int mask)
    {
        Vector3 randomPos = Random.insideUnitSphere * dist;
        randomPos += originPos;

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, dist, mask))
        {
            return hit.position;
        }
        return originPos;
    }

    public static float GetPathLenght(NavMeshPath path)
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
        Vector3 secondPos = startPos + shiftedLookDirection.normalized * lineLenght;
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
    public void UpdatePatrolPoint(PatrolPoint point)
    {
        if (point != interestPoint)
        {
        interestPoint = point;
        interestPointPos = interestPoint.position;
        }
    }
    public void ClearPatrolPoint()
    {
        interestPoint = null;
        interestPointPos = Vector3.zero;
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

        if (targetPos == null)
            return;

        Gizmos.color = Color.green;
        Vector3 origin = transform.position;

        Vector3 direction = shiftedLookDirection.normalized;
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
        Gizmos.DrawRay(origin, dynamicAxe * lookLenght);



    }
}
