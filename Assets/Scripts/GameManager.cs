using UnityEngine;
using UnityEngine.UI;
public class GameManager : MonoBehaviour
{
    public float timerForActivatingMiniGame;
    public float timerForGameOverMiniGame;
    public FirstPersonController player;
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
        timerForActivatingMiniGame += Time.deltaTime;
        if (timerForActivatingMiniGame >= 20f && sceneManager.IsUsingFirstCamera())
        {
            mark.gameObject.SetActive(true);
            monstersArriving = true;
            timerForGameOverMiniGame += Time.deltaTime;
        }
        if (timerForGameOverMiniGame >= 20f)
        {
            player.GameOver();
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
}
