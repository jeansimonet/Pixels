using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPatternView
    : PixelsApp.Page
{
    [Header("Controls")]
    public Button backButton;
    public InputField animationNameText;
    public Button saveButton;
    public RawImage previewImage;
    public UIParameterEnum animationSelector;
    public RectTransform parametersRoot;
    public Button playOnDieButton;

    public Animations.EditAnimation editAnimation { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    
    UIParameterManager.ObjectParameterList parameters;
    bool patternDirty = false;
    Presets.EditDie previewDie = null;
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
                DicePool.Instance.RequestDisconnectDie(connectedDie);
                connectedDie = null;
            });
        }
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

        animationSelector.Setup("Animation Type", () => editAnimation.type, (t) => SetAnimationType((Animations.AnimationType)t));

        // Setup all other parameters
        parameters = UIParameterManager.Instance.CreateControls(anim, parametersRoot);
        parameters.onParameterChanged += OnAnimParameterChanged;

        dieRenderer.rotating = true;
        dieRenderer.SetAnimation(anim);
        dieRenderer.Play(true);
    }

    void Awake()
    {
        backButton.onClick.AddListener(DiscardAndGoBack);
        animationNameText.onEndEdit.AddListener(newName => editAnimation.name = newName);
        saveButton.gameObject.SetActive(false);
        saveButton.onClick.AddListener(SaveAndGoBack);
        playOnDieButton.onClick.AddListener(PreviewOnDie);
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

            dieRenderer.rotating = true;
            dieRenderer.SetAnimation(newEditAnimation);

            editAnimation = newEditAnimation;
        }
    }

    void PreviewOnDie()
    {
        void handleError(string title, string message)
        {
            PixelsApp.Instance.ShowDialogBox(title, message, "Ok", null, null);
            connectedDie = null;
            previewDie = null;
        }

        void playOnConnectedDie()
        {
            PixelsApp.Instance.GetDieReady(connectedDie, (_, res2, errorMsg) =>
            {
                if (res2)
                {
                    connectedDie.SetLEDAnimatorMode(__ =>
                    {
                        var editSet = AppDataSet.Instance.ExtractEditSetForAnimation(editAnimation);
                        var dataSet = editSet.ToDataSet();
                        connectedDie.PlayTestAnimation(dataSet, (res3) =>
                        {
                            if (!res3)
                            {
                                DicePool.Instance.RequestDisconnectDie(connectedDie);
                                handleError("Transfer Error", "Could not play animation on " + previewDie.name);
                            }
                        });
                    });
                }
                else
                {
                    handleError("Connection Error", "Could not connect to " + previewDie.name);
                }
            });
        }

        void connectAndPlay()
        {
            if (connectedDie == null)
            {
                connectedDie = DicePool.Instance.FindDie(previewDie);
            }

            if (connectedDie != null)
            {
                playOnConnectedDie();
            }
            else
            {
                handleError("Missing Die", "Could not find die " + previewDie.name + " in dice bag");
            }
        }

        if (previewDie == null)
        {
            PixelsApp.Instance.ShowDiePicker("Select Die for Preview", null, (res, newDie) =>
            {
                if (res)
                {
                    previewDie = newDie;
                    connectAndPlay();
                }
                // Cancelled
            });
        }
        else
        {
            connectAndPlay();
        }
    }
}
