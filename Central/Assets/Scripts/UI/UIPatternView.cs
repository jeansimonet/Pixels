using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using Dice;

public class UIPatternView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Button backButton;
    public InputField animationNameText;
    public Button saveButton;
    public RawImage previewImage;
    public RotationSlider rotationSlider;
    public UIParameterEnum animationSelector;
    public RectTransform parametersRoot;
    public Button playOnDieButton;
    public UIRotationControl rotationControl;

    public Animations.EditAnimation editAnimation { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    
    UIParameterManager.ObjectParameterList parameters;
    bool patternDirty = false;
    EditDie previewDie = null;
    Die connectedDie = null;

    public override void Enter(object context)
    {
        base.Enter(context);
        var anim = context as Animations.EditAnimation;
        if (anim != null)
        {
            Setup(anim);
        }
        patternDirty = false;
        saveButton.gameObject.SetActive(false);
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
        if (DiceRendererManager.Instance != null && this.dieRenderer != null)
        {
            DiceRendererManager.Instance.DestroyDiceRenderer(this.dieRenderer);
            this.dieRenderer = null;
        }

        foreach (var parameter in parameters.parameters)
        {
            GameObject.Destroy(parameter.gameObject);
        }
        parameters = null;

        if (connectedDie != null)
        {
            connectedDie.SetStandardMode(_ =>
            {
                DicePool.Instance.DisconnectDie(connectedDie);
                connectedDie = null;
            });
        }
        previewDie = null;
    }

    void Setup(Animations.EditAnimation anim)
    {
        editAnimation = anim;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(anim.defaultPreviewSettings.design, 600);
        if (dieRenderer != null)
        {
            previewImage.texture = dieRenderer.renderTexture;
        }
        animationNameText.text = anim.name;

        rotationSlider.Setup(this.dieRenderer.die);
        rotationControl.Setup(this.dieRenderer.die);

        animationSelector.Setup("Animation Type", () => editAnimation.type, (t) => SetAnimationType((Animations.AnimationType)t));

        // Setup all other parameters
        parameters = UIParameterManager.Instance.CreateControls(anim, parametersRoot);
        parameters.onParameterChanged += OnAnimParameterChanged;

        dieRenderer.SetAuto(true);
        dieRenderer.SetAnimation(anim);
        dieRenderer.Play(true);
    }

    void Awake()
    {
        backButton.onClick.AddListener(DiscardAndGoBack);
        animationNameText.onEndEdit.AddListener(newName => editAnimation.name = newName);
        saveButton.gameObject.SetActive(false);
        saveButton.onClick.AddListener(SaveAndGoBack);
        playOnDieButton.onClick.AddListener(() => PreviewOnDie());
    }

    void DiscardAndGoBack()
    {
        if (patternDirty)
        {
            PixelsApp.Instance.ShowDialogBox(
                "Discard Changes",
                "You have unsaved changes, are you sure you want to discard them?",
                "Discard",
                "Cancel", discard => 
                {
                    if (discard)
                    {
                        // Reload from file
                        AppDataSet.Instance.LoadData();
                        NavigationManager.Instance.GoBack();
                    }
                });
        }
        else
        {
            NavigationManager.Instance.GoBack();
        }
    }

    void SaveAndGoBack()
    {
        Debug.Assert(patternDirty);
        AppDataSet.Instance.SaveData();
        NavigationManager.Instance.GoBack();
    }

    void OnAnimParameterChanged(EditObject animObject, UIParameter parameter, object newValue)
    {
        var theEditAnim = (Animations.EditAnimation)animObject;
        Debug.Assert(theEditAnim == editAnimation);
        dieRenderer.SetAnimation(theEditAnim);
        patternDirty = true;
        saveButton.gameObject.SetActive(true);
    }

    void SetAnimationType(Animations.AnimationType newType)
    {
        if (newType != editAnimation.type)
        {
            // Change the type, which really means create a new animation and replace the old one
            var newEditAnimation = Animations.EditAnimation.Create(newType);

            // Copy over the few things we can
            newEditAnimation.duration = editAnimation.duration;
            newEditAnimation.name = editAnimation.name;
            newEditAnimation.defaultPreviewSettings = editAnimation.defaultPreviewSettings;

            // Replace the animation
            AppDataSet.Instance.ReplaceAnimation(editAnimation, newEditAnimation);

            // Setup the parameters again
            foreach (var parameter in parameters.parameters)
            {
                GameObject.Destroy(parameter.gameObject);
            }

            parameters = UIParameterManager.Instance.CreateControls(newEditAnimation, parametersRoot);
            parameters.onParameterChanged += OnAnimParameterChanged;

            dieRenderer.SetAuto(true);
            dieRenderer.SetAnimation(newEditAnimation);

            editAnimation = newEditAnimation;
        }
    }

    Coroutine PreviewOnDie()
    {
        return StartCoroutine(PreviewOnDieCr());
    }

    IEnumerator PreviewOnDieCr()
    {
        if (previewDie == null)
        {
            bool? previewDieSelected = null;
            PixelsApp.Instance.ShowDiePicker(
                "Select Die for Preview",
                null,
                (ed, d) =>  true,
                (res, newDie) =>
                {
                    previewDie = newDie;
                    previewDieSelected = res;
                });
            yield return new WaitUntil(() => previewDieSelected.HasValue);
        }

        if (previewDie != null)
        {
            // Are we already connected?
            if (connectedDie == null)
            {
                string error = null;
                yield return DiceManager.Instance.ConnectDie(previewDie, (_, die, errorMsg) =>
                {
                    connectedDie = die;
                    error = errorMsg;
                });

                if (connectedDie == null)
                {
                    bool acknowledged = false;
                    PixelsApp.Instance.ShowDialogBox("Could not connect.", error, "Ok", null, _ => acknowledged = true);
                    yield return new WaitUntil(() => acknowledged);
                }
                else
                {
                    bool acknowledged = false;
                    connectedDie.SetLEDAnimatorMode(_ =>
                    {
                        acknowledged = true;
                    });
                    yield return new WaitUntil(() => acknowledged);
                }
            }

            if (connectedDie != null)
            {
                var editSet = AppDataSet.Instance.ExtractEditSetForAnimation(editAnimation);
                var dataSet = editSet.ToDataSet();
                bool playResult = false;
                yield return connectedDie.PlayTestAnimation(dataSet, (res) => playResult = res);
                if (!playResult)
                {
                    DicePool.Instance.DisconnectDie(connectedDie);
                    bool acknowledged = false;
                    PixelsApp.Instance.ShowDialogBox("Transfer Error", "Could not play animation on " + connectedDie.name + ", Transfer error", "Ok", null, _ => acknowledged = true);
                    connectedDie = null;
                    previewDie = null;
                    yield return new WaitUntil(() => acknowledged);
                }
            }
        }
    }
}
