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
    public InputField titleText;
    public Button saveButton;
    public RawImage diePreviewImage;
    public RawImage patternPreview;
    public Button loadFromFile;
    public Button reloadFromFile;

    // public UIColorEditor colorEditor;
    // public MultiSlider multiSlider;
    public SingleDiceRenderer dieRenderer { get; private set; }

    public bool isShown => gameObject.activeSelf;

	Texture2D _texture;

    EditPattern currentPattern;
    System.Action<bool, EditPattern> closeAction;
    string currentFilepath;
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

        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(Dice.DesignAndColor.V5_Black);
        if (dieRenderer != null)
        {
            diePreviewImage.texture = dieRenderer.renderTexture;
        }

        gameObject.SetActive(true);
        currentPattern = previousPattern;
        titleText.text = title;

        RepaintPreview();

        this.closeAction = closeAction;
        saveButton.gameObject.SetActive(true);
        reloadFromFile.interactable = false;
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
        reloadFromFile.onClick.AddListener(UpdateFromCurrentFile);
        titleText.onEndEdit.AddListener(newName => currentPattern.name = newName);
		_texture = new Texture2D(512, 20, TextureFormat.ARGB32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
    }

	void OnDestroy()
	{
		patternPreview.texture = null;
		Object.Destroy(_texture);
		_texture = null;
	}

    void Hide(bool result, EditPattern pattern)
    {
        if (this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }
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

    void FileSelected(string filePath)
    {
        Debug.Log("file path: " + filePath);
        currentFilepath = filePath;
        UpdateFromCurrentFile();
    }

    void UpdateFromCurrentFile()
    {
        byte[] fileData = File.ReadAllBytes(currentFilepath);
        var tex = new Texture2D(2, 2);
        tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        currentPattern.FromTexture(tex);
        currentPattern.name = System.IO.Path.GetFileNameWithoutExtension(currentFilepath);
        titleText.text = currentPattern.name;
        GameObject.Destroy(tex);
        isDirty = true;
        saveButton.gameObject.SetActive(true);
        reloadFromFile.interactable = false;
        RepaintPreview();
    }

    void LoadFromFile()
    {
        #if UNITY_EDITOR
            FileSelected(UnityEditor.EditorUtility.OpenFilePanel("Select png", "", "png"));
        #else
            NativeFilePicker.PickFile( FileSelected, new string[] { NativeFilePicker.ConvertExtensionToFileType( "png" ); } );
        #endif

        //var filePath = System.IO.Path.Combine(Application.persistentDataPath, $"pattern.png");
    }

    void RepaintPreview()
    {
		Object.Destroy(_texture);

        var anim = new EditAnimationGradientPattern();
        anim.name = "temp anim";
        anim.gradient = new EditRGBGradient();
        anim.gradient.keyframes.Add(new EditRGBKeyframe() { time = 0.0f, color = Color.white });
        anim.gradient.keyframes.Add(new EditRGBKeyframe() { time = 1.0f, color = Color.white });
        anim.pattern = currentPattern;
        anim.duration = currentPattern.duration;

        dieRenderer.SetAuto(true);
        dieRenderer.SetAnimation(anim);
        dieRenderer.Play(true);

        _texture = currentPattern.ToTexture();
        patternPreview.texture = _texture;        
    }
}
