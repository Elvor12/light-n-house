using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Button : MonoBehaviour
{
    private bool isActive = false;
    private float fading = 0;
    public Image image;
    public void OnButtonClick()
    {
        isActive = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (isActive)
        {
            fading += Time.deltaTime * 0.5f;
            if (fading >= 1)
            {
                isActive = false;
                SceneManager.LoadScene("Demo");
            }
            Color color = image.color;
            color.a = fading;
            image.color = color;
            
        }
        
    }
}
