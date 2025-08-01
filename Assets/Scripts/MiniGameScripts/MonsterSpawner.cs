using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject monsterPrefab;
    public GameObject root;
    public Transform[] spawnPoints;
    public ScenesManager scenesManager;
    public float spawnInterval = 5f;
    public int maxMonsters = 10;
    public bool isActive = false;
    public bool miniGameCleared = true;

    private float timer = 0f;
    private int currentMonsterCount = 0;
    public int remainedMonsters;
    void Update()
    {
        //Тут запускаются монстры только если камера переведена и монстры все еще остались. 
        // То есть когда ты убил всех монстров ты как бы еще можешь сидеть в миниигре но уже никто не появится.
        // Чтобы они опять появились нужно чтобы таймер опять обновился и ты перезашел на сцену(ниже написано почему)
        if (isActive && remainedMonsters > 0)
        {
            timer += Time.deltaTime;

            if (timer >= spawnInterval && currentMonsterCount < maxMonsters)
            {
                SpawnMonster();
                timer = 0f;
                //Тут говно. Это нужно переделать если тебе будет не в падлу. 
                // Счетчик уменьшяется когда монстры появляются.Когда ты выходишь из сцены, при этом кого-то не убив, монстр самоуничтожается и счетчик снова поднимается. 
                // Поэтому желательно чтобы счетчик уменьшался только тогда когда ты убиваешь кого-то не через выход из сцены. Мне просто не хотелось копаться в скрипте монстра 
                // чтобы там при их смерти счетчик уменьшался.
                remainedMonsters -= 1;
            }
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
    // Вот почему. этот метод запускается только когда ты заходишь на сцену. Ну и счетчик оставшихся монстров обновляются только если они закончились. 
    // Напоминаю что сцена может запуститься только если таймер закончился. 
    // В итоге оставшиеся монстры обновляются только если ты их уже закончил до этого и таймер обновился.
    public void ActivateMonstersEncounter()
    {
        if (remainedMonsters == 0)
        {
            remainedMonsters = Random.Range(1, 4);
        }
    }
}
