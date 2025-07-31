using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public GameObject root;
    public Transform[] spawnPoints;
    public float spawnInterval = 5f;
    public int maxMonsters = 10;

    private float timer = 0f;
    private int currentMonsterCount = 0;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentMonsterCount < maxMonsters)
        {
            SpawnMonster();
            timer = 0f;
        }
    }

    void SpawnMonster()
    {
        Transform spawnPoint;

        if (spawnPoints.Length > 0)
            spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        else
            spawnPoint = transform;

        GameObject monster = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity, root.transform);
        currentMonsterCount++;

        Monster monsterScript = monster.GetComponent<Monster>();
        if (monsterScript != null)
            monsterScript.OnMonsterDestroyed += OnMonsterDestroyed;
    }

    void OnMonsterDestroyed()
    {
        currentMonsterCount = Mathf.Max(0, currentMonsterCount - 1);
    }
}
