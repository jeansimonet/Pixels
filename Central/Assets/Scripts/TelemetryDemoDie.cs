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
    public Text faceNumberText;
    public ColorSelector selector;
    public Button playAnimButton;

    Die die;


    private void Awake()
    {
    }
    // Use this for initialization
    void Start()
    {
        selector.onMouseButtonUp.AddListener(() =>
        {
            die.SetLEDsToColor(ColorSelector.GetColor());
        });

        int count = 21;
        Button[] buttons = new Button[count];
        buttons[0] = playAnimButton;
        playAnimButton.GetComponentInChildren<Text>().text = "0";
        playAnimButton.onClick.AddListener(() => die.PlayAnimation(0));

        var root = playAnimButton.transform.parent;
        for (int i = 1; i < count; ++i)
        {
            var btn = GameObject.Instantiate<Button>(playAnimButton, root);
            btn.GetComponentInChildren<Text>().text = i.ToString();
            int animIndex = i;
            btn.onClick.AddListener(() =>
                {
                    die.PlayAnimation(animIndex);
                });
        }
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
 
        // Update the ui color
        //die.GetDefaultAnimSetColor((col) => UpdateUIColor(col));
    }

    public void OnTelemetryReceived(AccelFrame frame)
    {
        graphs.OnTelemetryReceived(frame);
        die3D.UpdateAcceleration(frame.acc);

    }

    void OnDieSettingsChanged(Die die)
    {
        nameField.text = die.name;
    }

    void UpdateUIColor(Color uiColor)
    {
        die3D.pipsColor = uiColor;
        faceNumberText.color = uiColor;
    }

    public void PlayAnim(int animIndex)
    {
        die.PlayAnimation(animIndex);
    }
}