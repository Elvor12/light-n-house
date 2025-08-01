using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public Camera firstCamera;
    public Camera secondCamera;
    public GameManager gameManager;
    public GameManagerMiniGame gameManagerMiniGame;
    private string secondScene = "MiniGame";
    void Start()
    {
        if (firstCamera != null)
        {
            firstCamera.enabled = true;
        }

        LoadSecondScene();
    }

    public bool IsUsingFirstCamera()
    {
        return firstCamera != null && firstCamera.enabled;
    }

    public bool IsUsingSecondCamera()
    {
        return secondCamera != null && secondCamera.enabled;
    }
    void LoadSecondScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(secondScene, LoadSceneMode.Additive);

        asyncLoad.completed += (AsyncOperation op) =>
        {
            Scene loadedScene = SceneManager.GetSceneByName(secondScene);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                GameObject[] objs = loadedScene.GetRootGameObjects();
                foreach (var obj in objs)
                {
                    Camera cam = obj.GetComponentInChildren<Camera>();
                    if (cam != null)
                    {
                        secondCamera = cam;
                        secondCamera.enabled = false;
                        break;
                    }
                    
                }
                foreach (var obj in objs)
                {
                    GameManagerMiniGame manager = obj.GetComponentInChildren<GameManagerMiniGame>();
                    if (manager != null) gameManagerMiniGame = manager;
                }
            }
        };
    }

    public void SwitchToSecond()
    {
        if (secondCamera != null && gameManager.monstersArriving)
        {
            firstCamera.enabled = false;
            secondCamera.enabled = true;
            gameManagerMiniGame.StartMiniGame();        
        }
    }

    public void SwitchToFirst()
    {
        if (firstCamera != null)
        {
            firstCamera.enabled = true;
            secondCamera.enabled = false;
            gameManager.IsMiniGameCleared(gameManagerMiniGame.StopMiniGame());
        }
    }
    public void InMiniGameCheck()
    {
        if (secondCamera.enabled == true)
        {
            SwitchToFirst();
        }
    }
    void Update()
    {
        
    }
}
