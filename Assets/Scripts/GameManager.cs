using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public float timerForActivatingMiniGame;
    public float timerForGameOverMiniGame;
    public float timerForchangingInterestPoint;
    public float globalMovedBoard = 100f;
    public readonly float globalBoard = 100f;
    public float rearrangeTimerForInterestPoint;
    public FirstPersonController player;
    public MonsterLogic monsterLogic;
    public ScenesManager sceneManager;
    public Image mark;
    public bool monstersArriving = false;
    void Start()
    {

    }

    // Это хаб таймеров))) Тут вроде все понятно. Если миниигра пройдена то таймер обнуляется.
    //  Если ты зашел в миниигру - таймер останавливается, если вышел не добив - продолжает идти.
    void Update()
    {
        globalMovedBoard -= Time.deltaTime;
        timerForchangingInterestPoint += Time.deltaTime;

        timerForActivatingMiniGame += Time.deltaTime;
        if (timerForActivatingMiniGame >= 10f && sceneManager.IsUsingFirstCamera())
        {
            mark.gameObject.SetActive(true);
            monstersArriving = true;
            timerForGameOverMiniGame += Time.deltaTime;
        }
        if (timerForGameOverMiniGame >= 20f)
        {
            player.GameOver();
        }
        if (timerForchangingInterestPoint >= globalMovedBoard)
        {
            monsterLogic.targetPointPos = player.GetClosestPatrolPoint().position;
            monsterLogic.timerForResidence = 0;
            timerForchangingInterestPoint = 0;
            globalMovedBoard = globalBoard;
        }
    }
    public void IsMiniGameCleared(bool isIt)
    {
        if (isIt)
        {
            timerForActivatingMiniGame = 0;
            timerForGameOverMiniGame = 0;
            monstersArriving = false;
            mark.gameObject.SetActive(false);
        }
    }
    public void CalculateUpperTimer()
    {
        Debug.Log($"here, {((timerForchangingInterestPoint / globalBoard) + 1) * globalMovedBoard - globalMovedBoard}, {timerForchangingInterestPoint}"  );

        globalMovedBoard = ((timerForchangingInterestPoint / globalBoard) + 1) * globalMovedBoard - timerForchangingInterestPoint;
        timerForchangingInterestPoint = 0;
    }
}
