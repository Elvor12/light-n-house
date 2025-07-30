using UnityEngine;
using UnityEngine.AI;

public class PatrolPoint : MonoBehaviour
{
    public float maxDistance = 1f;
    public float radious = 7;
    public Vector3 position;
    private Transform pointTransform;
    private MonsterLogic monsterLogic;
    private Transform monsterTransform;
    void Awake()
    {
        monsterLogic = FindAnyObjectByType<MonsterLogic>();
        pointTransform = GetComponent<Transform>();
        monsterTransform = monsterLogic.GetComponent<Transform>();
        NavMeshHit hit;
        if (NavMesh.SamplePosition(pointTransform.position, out hit, maxDistance, NavMesh.AllAreas))
        {
            position = hit.position;
            pointTransform.position = hit.position;
        }
        else Debug.Log("There is no place for my dot");
        monsterLogic.targetPointPos = position;
    }
    void Update()
    {
        if (monsterLogic.targetPointPos == position && monsterLogic.interestPoint != this)
        {
            if ((monsterTransform.position - position).sqrMagnitude < radious * radious)
            {
                if (MonsterLogic.GetPathLenght(monsterLogic.interestPath) < radious)
                {
                    monsterLogic.UpdatePatrolPoint(this);
                    Debug.Log(monsterLogic.interestPointPos);
                }
            }
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawSphere(position, 0.3f);
    }
}

