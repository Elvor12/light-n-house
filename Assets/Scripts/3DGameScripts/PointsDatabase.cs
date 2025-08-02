using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PointsDatabase : MonoBehaviour
{
    public Dictionary<int, PatrolPoint> points = new();
    public MonsterLogic monsterLogic;
    private NavMeshAgent agent;
    void Start()
    {
        agent = monsterLogic.GetComponent<NavMeshAgent>();
        points.Clear();
        PatrolPoint[] patrolPoints = GetComponentsInChildren<PatrolPoint>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            points[i] = patrolPoints[i];
        }
        foreach (var elem in points)
        {
            NavMeshPath path = new();
            
            if (!agent.CalculatePath(elem.Value.position, path) || path.status != NavMeshPathStatus.PathComplete)
                Debug.LogError("There is hole in navmeshWeb, Ivan, pls, fix it");
            Debug.Log($"{elem.Key}, {elem.Value}");
        }
    }

    public Vector3 GetNewTarget()
    {
        List<int> pointsKeys = new List<int>(points.Keys);
        int pointNumb = Random.Range(0, points.Keys.Count);
        return points[pointsKeys[pointNumb]].position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
