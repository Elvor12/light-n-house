using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerMiniGame : MonoBehaviour
{
    public MonsterSpawner monsterSpawner;

    private ScenesManager scenesManager;
    private FirstPersonController player;
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
    
    public void LoseMiniGame()
    {
        scenesManager = FindAnyObjectByType<ScenesManager>();
        player = FindAnyObjectByType<FirstPersonController>();
        if (scenesManager.IsUsingSecondCamera())
        {
            scenesManager.SwitchToFirst();
            player.GameOver();

        }
    }
}
