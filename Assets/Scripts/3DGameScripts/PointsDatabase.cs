using System.Collections.Generic;
using UnityEngine;

public class PointsDatabase : MonoBehaviour
{
    public Dictionary<int, PatrolPoint> points = new();
    void Start()
    {
        points.Clear();
        PatrolPoint[] patrolPoints = GetComponentsInChildren<PatrolPoint>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            points[i] = patrolPoints[i];
        }
        foreach (var elem in points)
        {
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
