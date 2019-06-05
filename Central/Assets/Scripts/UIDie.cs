using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDie : MonoBehaviour
{
	[SerializeField]
	UISpritesAsset sprites;
	[SerializeField]
	Image _diceFaceUI;
	[SerializeField]
	Text _diceNameUI;
	[SerializeField]
	Button _moreOptionsButton;
	//[SerializeField]
	//Text[] _diceStatsUI;

	public int faceNumber
	{
		get
		{
			return _faceNumber;
		}
		set
		{
			_faceNumber = value;

			// Update the display
			if (_faceNumber >= 0) {
				_diceFaceUI.sprite = sprites.faceSprites [_faceNumber];
			} else {
				_diceFaceUI.sprite = sprites.unknownSprite;
			}
		}
	}
	int _faceNumber;

	public string diceName
	{
		get
		{
			return _die.name;
		}
	}

    public Die die
    {
        get { return _die; }
        set
        {
            _die = value;
            if (_die != null && _diceNameUI != null)
            {
                // Update the display
                _diceNameUI.text = _die.name;
            }
        }
    }
    Die _die;

	public void SetFacePercent(int face, float percent)
	{
		//_diceStatsUI[face].text = (face + 1).ToString() + ": " + (percent * 100).ToString("F") + "%";
	}

	public delegate void OnMoreOptionsDelegate();
	public event OnMoreOptionsDelegate OnMoreOptions;

	void Awake()
	{
		if (_moreOptionsButton != null)
			_moreOptionsButton.onClick.AddListener(() => { if (OnMoreOptions != null) OnMoreOptions(); });
	}

	// Use this for initialization
	void Start ()
	{
		for (int i = 0; i < 6; ++i)
		{
			SetFacePercent(i, 0.0f);
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
