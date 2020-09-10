using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Presets;
using Dice;

public class UIPatternView
    : UIPage
{
    [Header("Controls")]
    public RawImage previewImage;
    public RotationSlider rotationSlider;
    public UIParameterEnum animationSelector;
    public RectTransform parametersRoot;
    public Button playOnDieButton;
    public UIRotationControl rotationControl;

    public Animations.EditAnimation editAnimation { get; private set; }
    public SingleDiceRenderer dieRenderer { get; private set; }
    
    UIParameterManager.ObjectParameterList parameters;
    EditDie previewDie = null;

    public override void Enter(object context)
    {
        base.Enter(context);
        var anim = context as Animations.EditAnimation;
        if (anim != null)
        {
            Setup(anim);
        }
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

        if (previewDie != null)
        {
            if (previewDie.die != null)
            {
                previewDie.die.SetStandardMode();
                DiceManager.Instance.DisconnectDie(previewDie);
                previewDie = null;
            }
            else
            {
                previewDie = null;
            }
        }
    }

    void Setup(Animations.EditAnimation anim)
    {
        base.SetupHeader(false, false, anim.name, SetName);
        editAnimation = anim;
        this.dieRenderer = DiceRendererManager.Instance.CreateDiceRenderer(anim.defaultPreviewSettings.design, 600);
        if (dieRenderer != null)
        {
            previewImage.texture = dieRenderer.renderTexture;
        }

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
        playOnDieButton.onClick.AddListener(() => PreviewOnDie());
    }

    void OnAnimParameterChanged(EditObject animObject, UIParameter parameter, object newValue)
    {
        var theEditAnim = (Animations.EditAnimation)animObject;
        Debug.Assert(theEditAnim == editAnimation);
        dieRenderer.SetAnimation(theEditAnim);
        base.pageDirty = true;
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
            base.pageDirty = true;
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
                (ed) =>  true,
                (res, newDie) =>
                {
                    previewDie = newDie;
                    previewDieSelected = res;
                });
            yield return new WaitUntil(() => previewDieSelected.HasValue);
        }

        if (previewDie != null)
        {
            if (previewDie.die == null)
            {
                string error = null;
                yield return DiceManager.Instance.ConnectDie(previewDie, (_, res, errorMsg) =>
                {
                    error = errorMsg;
                });
                if (error != null)
                {
                    bool acknowledged = false;
                    PixelsApp.Instance.ShowDialogBox("Could not connect.", error, "Ok", null, _ => acknowledged = true);
                    yield return new WaitUntil(() => acknowledged);
                }
                else
                {
                    previewDie.die.SetLEDAnimatorMode();
                }
            }

            if (previewDie.die != null)
            {
                var editSet = AppDataSet.Instance.ExtractEditSetForAnimation(editAnimation);
                var dataSet = editSet.ToDataSet();
                bool playResult = false;
                yield return previewDie.die.PlayTestAnimation(dataSet, (res) => playResult = res);
                if (!playResult)
                {
                    bool acknowledged = false;
                    PixelsApp.Instance.ShowDialogBox("Transfer Error", "Could not play animation on " + previewDie.name + ", Transfer error", "Ok", null, _ => acknowledged = true);
                    previewDie = null;
                    yield return new WaitUntil(() => acknowledged);
                }
            }
        }
    }

    void SetName(string newName)
    {
        editAnimation.name = newName;
        base.pageDirty = true;
    }
}
