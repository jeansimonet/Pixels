using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddDiceToPoolDice : MonoBehaviour
{
    public Text nameText;
    public Button selectButton;
    public Image selectedImage;

	public Die die { get; private set; }
	public delegate void OnSelectionChanged(Die die, bool selected);
	public OnSelectionChanged onSelected;

    private bool selected;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Setup(Die die)
    {
        this.die = die;
        selected = false;
        onSelected = null;
        nameText.text = die.name;
        selectedImage.gameObject.SetActive(false);
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() =>
        {
            selected = !selected;
            if (selected)
            {
                selectedImage.gameObject.SetActive(true);
            }
            else
            {
                selectedImage.gameObject.SetActive(false);
            }
            if (onSelected != null)
            {
                onSelected(die, true);
            }
        });
    }
}
