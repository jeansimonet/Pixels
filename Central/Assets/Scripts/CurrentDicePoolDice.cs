using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class CurrentDicePoolDice
    : MonoBehaviour
{
    public Text nameText;
    public Text statusText;
    public Image diceImage;
    public Button diceButton;
    public CanvasGroup commandGroup;
    public Button renameButton;
    public Button forgetButton;
    public Button flashButton;
    public Button setColorButton;
    public RenameDieDialog renameDieDialog;

    public Die die { get; private set; }
    public Central central;
    bool commandsShown
    {
        get
        {
            return commandGroup.interactable;
        }
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Setup(Die die, Central central)
    {
        if (this.die != null)
        {
            die.OnSettingsChanged -= OnDieSettingsChanged;
        }

        this.die = die;
        this.central = central;
        nameText.text = die.name;
        statusText.text = die.connected ? "Ready" : "Missing";
        diceImage.color = Color.white;
        HideCommands();

        diceButton.onClick.RemoveAllListeners();
        diceButton.onClick.AddListener(ShowHideCommands);

        this.die.OnSettingsChanged += OnDieSettingsChanged;
    }

    void ShowHideCommands()
    {
        if (commandsShown)
        {
            HideCommands();
        }
        else
        {
            ShowCommands();
        }
    }

    void ShowCommands()
    {
        commandGroup.interactable = true;
        commandGroup.blocksRaycasts = true;
        commandGroup.alpha = 1.0f;

        // Register commands
        forgetButton.onClick.RemoveAllListeners();
        forgetButton.onClick.AddListener(ForgetDie);

        renameButton.onClick.RemoveAllListeners();
        renameButton.onClick.AddListener(RenameDie);

        flashButton.onClick.RemoveAllListeners();
        flashButton.onClick.AddListener(FlashDie);

        setColorButton.onClick.RemoveAllListeners();
        setColorButton.onClick.AddListener(SetNewDieColor);

    }

    void HideCommands()
    {
        commandGroup.interactable = false;
        commandGroup.blocksRaycasts = false;
        commandGroup.alpha = 0.0f;
    }

    void ForgetDie()
    {
        // Tell central to forget about this die
        central.ForgetDie(die, null);
        HideCommands();
    }

    void RenameDie()
    {
        var set = Animations.EditAnimationSet.CreateTestSet();
        Debug.Log(set.ToString());
        var dieSet = set.ToAnimationSet();

        StartCoroutine(die.UploadAnimationSet(dieSet));
        HideCommands();
    }

    void FlashDie()
    {
        HideCommands();
        die.Flash(0);
    }

    void SetNewDieColor()
    {
        HideCommands();
        die.SetNewColor(null);
    }

    void UpdateDieName()
    {
        nameText.text = die.name;
    }

    void OnDieSettingsChanged(Die die)
    {
        UpdateDieName();
    }
}
