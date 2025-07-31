using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    public Camera firstCamera;
    private Camera secondCamera;
    private string secondScene = "MiniGame";
    void Start()
    {
        if (firstCamera != null)
        {
            firstCamera.enabled = true;
        }

        LoadSecondScene();
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
            }
        };
    }

    public void SwitchToSecond()
    {
        if (secondCamera != null)
        {
            firstCamera.enabled = false;
            secondCamera.enabled = true;
        }
    }

    void Update()
    {
        
    }
}
