using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Animations;
using System.IO;

public class UIPatternEditor : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public Button saveButton;
    public RawImage patternPreview;
    public Button loadFromFile;

    // public UIColorEditor colorEditor;
    // public MultiSlider multiSlider;

    public bool isShown => gameObject.activeSelf;

	Texture2D _texture;

    EditPattern currentPattern;
    System.Action<bool, EditPattern> closeAction;
    public bool isDirty { get; private set; }

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, EditPattern previousPattern, System.Action<bool, EditPattern> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Color picker still active");
            ForceHide();
        }

        gameObject.SetActive(true);
        currentPattern = previousPattern.Duplicate();
        titleText.text = title;

        RepaintPreview();

        // multiSlider.FromGradient(currentGradient);
        // multiSlider.HandleSelected += OnHandleSelected;
		// multiSlider.SelectHandle(multiSlider.AllHandles[0]);
        // colorEditor.onColorSelected += OnColorSelected;

        this.closeAction = closeAction;
        saveButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentPattern);
    }

    void Awake()
    {
        backButton.onClick.AddListener(DiscardAndBack);
        saveButton.onClick.AddListener(SaveAndBack);
        loadFromFile.onClick.AddListener(LoadFromFile);
		_texture = new Texture2D(512, 20, TextureFormat.ARGB32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        patternPreview.texture = _texture;        
    }

	void OnDestroy()
	{
		patternPreview.texture = null;
		Object.Destroy(_texture);
		_texture = null;
	}

    void Hide(bool result, EditPattern pattern)
    {
        gameObject.SetActive(false);
        closeAction?.Invoke(result, pattern);
        closeAction = null;
    }

    void SaveAndBack()
    {
        Hide(true, currentPattern);
    }

    void DiscardAndBack()
    {
        Hide(false, currentPattern);
    }

    void LoadFromFile()
    {
        var filePath = System.IO.Path.Combine(Application.persistentDataPath, $"pattern.png");
        byte[] fileData = File.ReadAllBytes(filePath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        currentPattern.FromTexture(tex);
        GameObject.Destroy(tex);
        RepaintPreview();
        isDirty = true;
        saveButton.gameObject.SetActive(true);
    }

    void RepaintPreview()
    {
		Color[] pixels = new Color[512 * 20];
        for (int i = 0; i < pixels.Length; ++i)
        {
            pixels[i] = Color.black;
        }

        for (int j = 0; j < currentPattern.gradients.Count; ++j)
        {
            var currentGradient = currentPattern.gradients[j];
            int x = 0, lastMax = 0;
            for (int i = 1; i < currentGradient.keyframes.Count; ++i)
            {
                int max = Mathf.RoundToInt(currentGradient.keyframes[i].time / 0.02f);
                for (; x < max; ++x)
                {
                    Color prevColor = new Color(currentGradient.keyframes[i - 1].intensity, currentGradient.keyframes[i - 1].intensity, currentGradient.keyframes[i - 1].intensity);
                    Color nextColor = new Color(currentGradient.keyframes[i].intensity, currentGradient.keyframes[i].intensity, currentGradient.keyframes[i].intensity);
                    pixels[j * _texture.width + x] = Color.Lerp(prevColor, nextColor, ((float)x - lastMax) / (max - lastMax));
                }
                lastMax = max;
            }
        }
		_texture.SetPixels(pixels);
		_texture.Apply(false);
    }
}
