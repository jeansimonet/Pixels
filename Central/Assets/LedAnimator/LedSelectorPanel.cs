using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LedSelectorPanel : SingletonMonoBehaviour<LedSelectorPanel>
{
	[SerializeField]
	RectTransform _ledsRoot = null;

	System.Action<Sprite> _doneCb;

	public void Show(System.Action<Sprite> doneCB)
	{
		_doneCb = doneCB;
		gameObject.SetActive(true);
	}

	public void Close()
	{
		DoClose();
	}

	public Sprite GetLedSprite(string spriteName)
	{
		return _ledsRoot.GetComponentsInChildren<Image>().Select(s => s.sprite).FirstOrDefault(s => s.name == spriteName);
	}

	void DoClose(Sprite sprite = null)
	{
		gameObject.SetActive(false);
		_doneCb(sprite);
	}

	// Use this for initialization
	void Start()
	{
		foreach (var btn in _ledsRoot.GetComponentsInChildren<Button>())
		{
			btn.onClick.AddListener(() => OnLedButtonClick(btn));
		}
	}

	// Update is called once per frame
	void Update()
	{

	}

	void OnLedButtonClick(Button btn)
	{
		DoClose(btn.image.sprite);
	}
}
