using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LedSelector : MonoBehaviour
{
	[SerializeField]
	RectTransform _ledsRoot = null;

	System.Action<Sprite> _doneCb;

	static LedSelector _instance;
	public static LedSelector Instance { get { if (_instance == null) FindInstance(); return _instance; } }

	static void FindInstance()
	{
		_instance = Object.FindObjectOfType<Canvas>().rootCanvas.GetComponentInChildren<LedSelector>(includeInactive: true);
	}

	public void PickLed(System.Action<Sprite> doneCB)
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

	void OnDestroy()
	{
		_instance = null;
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
