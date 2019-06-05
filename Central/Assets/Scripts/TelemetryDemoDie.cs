using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TelemetryDemoDie : MonoBehaviour
{
    public Text nameField;
    public TelemetryDie graphs;
    public RawImage dieImage;
    public Die3D die3D;
    public Button changeColorButton;
    public Button showOffButton;
    public Button showOff2Button;
    public Text faceNumberText;

    Die die;


    private void Awake()
    {
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (die.face != -1)
            faceNumberText.text = (die.face).ToString();
        else
            faceNumberText.text = "";
    }

    public void Setup(Die die)
    {
        if (this.die != null)
        {
            this.die.OnSettingsChanged -= OnDieSettingsChanged;
        }

        nameField.text = die.name;
        graphs.Setup(die.name);
        var rt = die3D.Setup(1);
        dieImage.texture = rt;

        this.die = die;
        this.die.OnSettingsChanged += OnDieSettingsChanged;
 
        changeColorButton.onClick.RemoveAllListeners();
        changeColorButton.onClick.AddListener(ChangeColor);

        showOffButton.onClick.RemoveAllListeners();
        showOffButton.onClick.AddListener(() => ShowOff(1));

        showOff2Button.onClick.RemoveAllListeners();
        showOff2Button.onClick.AddListener(() => ShowOff(0));

        changeColorButton.interactable = true;
        showOffButton.interactable = true;
        showOff2Button.interactable = true;

        // Update the ui color
        die.GetDefaultAnimSetColor((col) => UpdateUIColor(col));
    }

    public void OnTelemetryReceived(Vector3 acc, int millis)
    {
        graphs.OnTelemetryReceived(acc, millis);
        die3D.UpdateAcceleration(acc);

    }

    void OnDieSettingsChanged(Die die)
    {
        nameField.text = die.name;
    }

    void ChangeColor()
    {
        StartCoroutine(ChangeColorCr());
    }

    IEnumerator ChangeColorCr()
    {
        // Disable buttons
        changeColorButton.interactable = false;
        showOffButton.interactable = false;
        showOff2Button.interactable = false;

        try
        {
            Color color = Color.white;
            die.SetNewColor((col) => color = col);
            yield return new WaitForSeconds(2.0f);
            UpdateUIColor(color);
        }
        finally
        {
        }
        changeColorButton.interactable = true;
        showOffButton.interactable = true;
        showOff2Button.interactable = true;
    }

    void UpdateUIColor(Color uiColor)
    {
        die3D.pipsColor = uiColor;
        faceNumberText.color = uiColor;
    }

    void ShowOff(int index)
    {
        StartCoroutine(FlashCr(index));
    }

    IEnumerator FlashCr(int index)
    {
        // Disable buttons
        changeColorButton.interactable = false;
        showOffButton.interactable = false;
        showOff2Button.interactable = false;
        Debug.Log("Showing Off");
        try
        {
            die.Flash(index);
            Debug.Log("Waiting");
            yield return new WaitForSeconds(2.0f);
        }
        finally
        {
        }

        Debug.Log("Resetting Buttons");
        changeColorButton.interactable = true;
        showOffButton.interactable = true;
        showOff2Button.interactable = true;
    }
}