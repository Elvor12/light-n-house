using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class MonsterLogic : MonoBehaviour
{

    public GameObject target;
    public float wanderDist = 10f;
    public float setWanderTimer = 100f;
    public LineRenderer linerender;
    public float lineLenght;
    public Vector3 direction = Vector3.zero;
    public Vector3 destination = Vector3.zero;

    public float maxDist = 5;
    public float minDist = 2;
    public float timer;
    public float viewTimer = 2f;
    private Transform targetPos;
    private NavMeshAgent agent;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPos = target.GetComponent<Transform>();
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
        Vector3 velocity = agent.velocity;

        if(destination != Vector3.zero) direction = Vector3.Lerp(direction, destination - transform.position, viewTimer * Time.deltaTime).normalized;

        SetLine();

        timer -= Time.deltaTime;

        NavMeshPath path = new NavMeshPath();

        if(agent.CalculatePath(targetPos.position, path) && path.status == NavMeshPathStatus.PathComplete) 
        {
            destination = targetPos.position;
        }
        else if ((timer <= 0 && !agent.hasPath))
        {   
            Vector3 target = ValidNavMeshPoint(transform.position, wanderDist, -1, maxDist, minDist, agent);
            destination = target;   
            timer = setWanderTimer;
        }
        
        if(Vector3.Distance(direction, (destination - transform.position).normalized) < 0.1f) 
        {
            agent.SetDestination(destination);
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
    private void SetLine() 
    {
        Vector3 startPos = transform.position;
        Vector3 secondPos = startPos + direction.normalized * lineLenght;
        linerender.positionCount = 2;
        linerender.SetPosition(0, startPos);
        linerender.SetPosition(1, secondPos);
    }
}
