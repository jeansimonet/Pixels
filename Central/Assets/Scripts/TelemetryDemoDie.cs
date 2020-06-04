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
        faceNumberText.color = uiColor;
    }

    public void PlayAnim(int animIndex)
    {
        die.PlayAnimation(animIndex);
    }
}