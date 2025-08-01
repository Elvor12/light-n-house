using System.Linq;
using UnityEngine;

public class GameManagerMiniGame : MonoBehaviour
{
    public MonsterSpawner monsterSpawner;
    void Start()
    {
        monsterSpawner = FindAnyObjectByType<MonsterSpawner>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    //Переходный скрипт для удобства. По сути GameManager обновляет данные за счет scene Manager, который в свою очередь получает данные отсюда.
    //То есть сцен менеджер имеет доступ в остальные два менеджера, и все операции производятся через него.
    public void StartMiniGame()
    {
        monsterSpawner.ActivateMonstersEncounter();
        monsterSpawner.isActive = true;
    }
    public bool StopMiniGame()
    {
        monsterSpawner.isActive = false;
        Monster[] monsters = FindObjectsByType<Monster>(FindObjectsSortMode.None);
        monsterSpawner.remainedMonsters += monsters.Count();
        foreach (var monster in monsters)
        {
            Destroy(monster.gameObject);
        }
        return monsterSpawner.remainedMonsters == 0;
    }
}
