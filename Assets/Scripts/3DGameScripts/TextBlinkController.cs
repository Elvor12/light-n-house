using UnityEngine;
using UnityEngine.UI;

public class TextBlinkController : MonoBehaviour
{
    public float blinkDuration = 2f;
    public float blinkSpeed = 5f;
    public float fadeDuration = 1f;

    private Text currentText = null;
    private float timer = 0f;
    private bool isFading = false;
    private bool isActive = false;

    void Update()
    {
        if (!isActive || currentText == null)
            return;

        if (!isFading)
        {
            if (timer < blinkDuration)
            {
                timer += Time.deltaTime;
                float alpha = (Mathf.Sin(timer * blinkSpeed * Mathf.PI * 2) + 1f) / 2f;
                SetAlpha(currentText, alpha);
            }
            else
            {
                isFading = true;
                timer = 0f;
            }
        }
        else
        {
            if (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                SetAlpha(currentText, alpha);
            }
            else
            {
                SetAlpha(currentText, 0f);
                isActive = false;
                currentText = null;
            }
        }
    }

    private void SetAlpha(Text txt, float alpha)
    {
        Color c = txt.color;
        c.a = Mathf.Clamp01(alpha);
        txt.color = c;
    }


    public void BlinkText(Text targetText)
    {
        if (targetText == null) return;

        currentText = targetText;
        timer = 0f;
        isFading = false;
        isActive = true;
        SetAlpha(currentText, 1f);
    }


    public void StopBlink()
    {
        if (currentText != null)
        {
            SetAlpha(currentText, 0f);
        }

        isActive = false;
        isFading = false;
        currentText = null;
        timer = 0f;
    }

    public bool IsBlinking()
    {
        return isActive;
    }
}
