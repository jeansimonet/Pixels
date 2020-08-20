using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgrammingBox : MonoBehaviour
{
    [Header("Controls")]
    public Text description;
    public Image prograssBarBackground;
    public Image progressBar;
    public float defaultProgressSpeed = 1.0f; // pct/sec

    public bool isShown => gameObject.activeSelf;
    float currentProgress = 0.0f;
    float currentDisplayProgress = 0.0f;
    float currentProgressSpeed = 1.0f;
    float lastUpdateTime = 0.0f;

    public void Show(string text)
    {
        this.gameObject.SetActive(true);
        currentProgress = 0.0f;
        currentDisplayProgress = 0.0f;
        currentProgressSpeed = defaultProgressSpeed;
        lastUpdateTime = Time.time;
        SetProgress(0, text);
        var offsetMax = progressBar.rectTransform.offsetMax;
        offsetMax.x = prograssBarBackground.rectTransform.rect.width * currentDisplayProgress;
        progressBar.rectTransform.offsetMax = offsetMax;
    }

    public void SetProgress(float newProgress, string text = null)
    {
        float deltaTime = Time.time - lastUpdateTime;
        if (deltaTime > 0.001f)
        {
            float deltaProgress = newProgress - currentDisplayProgress;
            currentProgressSpeed = deltaProgress / deltaTime;
            lastUpdateTime = Time.time;
        }
        currentProgress = newProgress;
        if (text != null)
        {
            description.text = text;
        }
    }

    void Update()
    {
        float maxDelta = currentProgressSpeed * Time.deltaTime;
        currentDisplayProgress = Mathf.MoveTowards(currentDisplayProgress, currentProgress, maxDelta);
        var offsetMax = progressBar.rectTransform.offsetMax;
        offsetMax.x = prograssBarBackground.rectTransform.rect.width * currentDisplayProgress;
        progressBar.rectTransform.offsetMax = offsetMax;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

}
