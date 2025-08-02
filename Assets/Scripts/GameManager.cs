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
    private AudioSource sound;
    public AudioClip firstSound;
    public AudioClip secondSound;
    public bool firstSoundStarted;

    public bool monstersArriving = false;
    void Start()
    {
        sound = GetComponent<AudioSource>();
    }

    // Это хаб таймеров))) Тут вроде все понятно. Если миниигра пройдена то таймер обнуляется.
    //  Если ты зашел в миниигру - таймер останавливается, если вышел не добив - продолжает идти.
    void Update()
    {
        globalMovedBoard -= Time.deltaTime;
        timerForchangingInterestPoint += Time.deltaTime;

        timerForActivatingMiniGame += Time.deltaTime;
        if (timerForActivatingMiniGame >= 20f && sceneManager.IsUsingFirstCamera())
        {
            if (!firstSoundStarted && !sound.isPlaying) ActivateFirstSound();
            mark.gameObject.SetActive(true);
            monstersArriving = true;
            timerForGameOverMiniGame += Time.deltaTime;
        }
        
        if (timerForGameOverMiniGame >= 20f)
        {
            sound.enabled = false;
            player.GameOver();
        }
        if (timerForchangingInterestPoint >= globalMovedBoard)
        {
            firstSoundStarted = false;
            monsterLogic.targetPointPos = player.GetClosestPatrolPoint().position;
            monsterLogic.timerForResidence = 0;
            timerForchangingInterestPoint = 0;
            globalMovedBoard = globalBoard;
        }
        if (firstSoundStarted && !sound.isPlaying)
        {
            sound.loop = true;
            sound.clip = secondSound;
            sound.Play();
            
        }
        if (!sceneManager.IsUsingFirstCamera())
        {
            sound.Stop();
        }
        else if (sound.clip != null && !sound.isPlaying)
        {
            sound.clip = secondSound;
            sound.Play();
        }
    }
    public void ActivateFirstSound()
    {
        firstSoundStarted = true;
        sound.clip = firstSound;
        sound.loop = false;
        sound.Play();

    }
    public void IsMiniGameCleared(bool isIt)
    {
        if (isIt)
        {
            sound.loop = false;
            sound.clip = null;
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
