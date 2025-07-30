using System.Collections.Generic;
using UnityEngine;

public class PointsDatabase : MonoBehaviour
{
    public Dictionary<int, Vector3> points = new();
    void Start()
    {
        points.Clear();
        PatrolPoint[] patrolPoints = GetComponentsInChildren<PatrolPoint>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            points[i] = patrolPoints[i].position;
        }
        foreach (var elem in points)
        {
            Debug.Log($"{elem.Key}, {elem.Value}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
