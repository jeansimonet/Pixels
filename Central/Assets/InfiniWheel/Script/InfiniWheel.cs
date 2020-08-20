using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Infini wheel.
/// </summary>
public class InfiniWheel : MonoBehaviour {

	/// <summary>
	/// Value changed gets fired when the currently selected item changes.
	/// If an AudioSource is assigned to click, it plays the sound.
	/// </summary>
	public event Action< int, Text > ValueChange;
	protected void OnValueChange (int id, Text t, bool sound = true) {
		if (click != null && !click.isPlaying && sound)
			click.Play();
		if (ValueChange != null)
			ValueChange (id, t);
	}

	/// <summary>
	/// The wheel item prefab.
	/// </summary>
	public GameObject wheelItemPrefab;

	/// <summary>
	/// The content object that stores the wheel items. This gets populated automatically when Init is called
	/// </summary>
	public GameObject content;

	/// <summary>
	/// The time it takes to snap the closest item to the middle when the wheel is no longer moving
	/// </summary>
	public float snapTime = 0.5f;

	/// <summary>
	/// Optional click sound. Plays every time the value changes 
	/// </summary>
	public AudioSource click;

	/// <summary>
	/// The amount of visible items on the wheel. Has to be an odd number.
	/// For example: if the itemCount is 5, it shows 2 items above the middle and 2 items below the middle.
	/// Note: This is not the maximum number of values that can be shown on the wheel!
	/// </summary>
	public int itemCount = 5;

	/// <summary>
	/// The height of the prefab. Used for positioning the wheel items correctly. Gets set automatically on Init.
	/// </summary>
	protected float prefabHeight;

	/// <summary>
	/// Stores the data that is used to populate the wheel items. Is set on Init and SetData.
	/// </summary>
	protected string[] itemData;

	/// <summary>
	/// The RectTransform of the content. Used for determining the position to snap to. Gets set automatically on Awake.
	/// </summary>
	protected RectTransform contentRect;

	/// <summary>
	/// The ScrollRect of this wheel. Used to check and set the velocity when braking / snapping.
	/// </summary>
	protected ScrollRect _sr;

	/// <summary>
	/// The list of current wheel items that are visible.
	/// </summary>
	protected List < WheelItem > _items;

	/// <summary>
	/// The current selection. Gets updated by UpdateContent. 
	/// </summary>
	protected int _curSelection = 0;
	public int SelectedItem {
		get {
			return _curSelection;
		}
	}

	/// <summary>
	/// Gets the current value.
	/// </summary>
	/// <value>The current value.</value>
	public string CurrentValue 
	{
		get {
			return itemData[_curSelection];
		}
	}
	
	void Awake () {
		contentRect = content.GetComponent<RectTransform>();
		if (itemCount % 2 == 0)
		{	
			itemCount++;
			Debug.LogWarning("Warning: itemCount can not be even. Changing it to " + itemCount);
		}
	}

	/// <summary>
	/// The Brake_R coroutine gets called by the Brake method. 
	/// It checks the velocity of the ScrollRect and if below a certain threshold, it starts moving the closest wheel item to the middle.
	/// </summary>
	bool _braking = false;
	IEnumerator Brake_R () {
		_braking = true;
		//check if the velocity is below a certain threshold.
		while (_braking && Mathf.Abs(_sr.velocity.y) >= 50f)
			yield return null;

		//determine the position of the middle item relative to the scroll rect.
		int middle = (itemCount -1) / 2;
		Vector3 start = content.transform.localPosition;
		Vector3 target = -_items [middle].transform.localPosition;
		float timer = snapTime;

		//move the content towards the target position.
		while (_braking && timer > 0f)
		{
			content.transform.localPosition = start + (target - start) * (1f - (timer / snapTime)) ;
			timer -= Time.deltaTime;
			yield return null;
		}
		//At the end, set is to the correct position and call OnValueChange.
		if (_braking)
		{
			_sr.velocity = Vector2.zero;
			content.transform.localPosition = target;	
			OnValueChange (_items [middle].ItemIndex, _items [middle].ItemText);
			_braking = false;
		}
	}

	/// <summary>
	/// Stops the brake coroutine. Gets called by the event trigger, when you start dragging the ScrollRect again. 
	/// </summary>
	public void CancelBrake () {
		_braking = false;
	}

	/// <summary>
	/// Starts the brake coroutine. Gets called by the event trigger when you stop dragging the ScrollRect.  
	/// </summary>
	public void Brake () {
		StartCoroutine (Brake_R ());
	}

	/// <summary>
	/// Init the wheel. Instantiates itemCount wheelItemPrefabs and puts them in the right position.
	/// Then populates the visible wheelItems by calling SetData.
	/// </summary>
	/// <param name="choices">A list of all the possible values (strings) on the wheel</param>
	public void Init(params string[] choices)
	{
		if (_items == null)
			_items = new List<WheelItem>();
		if (_sr == null)
			_sr = GetComponent<ScrollRect>();

		prefabHeight = (wheelItemPrefab.transform as RectTransform).sizeDelta.y;

		itemData = choices;

		//create itemCount wheelItemPrefabs and put them in the right positions.
		int start = -(itemCount - 1) / 2;
		int end = (itemCount + 1) / 2;

		for (int i = start; i < end; i++)
		{
			WheelItem wi = AddWheelItem(i);
			_items.Add(wi);
		}

		SetData(choices);
	}
	
	/// <summary>
	/// Instatiate a WheelItem (based on wheelItemPrefab) and adds it to content.transform 
	/// </summary>
	/// <param name="offset">Offset the item height in offset * prefabHeight from the center.</param>
	/// <returns>The newly initiated WheelItem</returns>
	protected WheelItem AddWheelItem(int offset)
	{
		wheelItemPrefab.SetActive(true);
		GameObject go = (GameObject)Instantiate(wheelItemPrefab);
		go.name = offset.ToString();
		RectTransform r = go.GetComponent<RectTransform>();

		r.SetParent(content.transform,false);
		go.transform.localPosition = new Vector2(0,-(offset)* prefabHeight);
		WheelItem wi = go.GetComponent<WheelItem>();
		wheelItemPrefab.SetActive(false);
		return wi;
	}

	/// <summary>
	/// Puts the right values on the visible wheel items.
	/// </summary>
	/// <param name="choices">A list of all the possible values (strings) on the wheel</param>
	public virtual void SetData (string[] choices)
	{
		itemData = choices;
		if (_items == null || _items.Count <= 0)
			Init(choices);
		if (_curSelection >= choices.Length)
			_curSelection = choices.Length -1;

		int start = -(itemCount-1) / 2;
		int end = (itemCount +1) / 2;

		//put the right values on the visible wheel items
		for (int i = start; i < end; i++)
		{
			int dataIndex = getDataIndex(_curSelection + i);
			_items[i-start].Init(itemData[dataIndex], dataIndex);
		}
	}

	/// <summary>
	/// Select the specified index.
	/// </summary>
	/// <param name="index">Index</param>
	public virtual void Select (int index) {	
		if (itemData == null || _items == null)
			return;

		int start = -(itemCount-1) / 2;
		int end = (itemCount +1) / 2;
		int middle = (itemCount -1) / 2;
		_curSelection = getDataIndex(index);
		for (int i = start; i < end; i++)
		{
			int dataIndex = getDataIndex(_curSelection + i);
			_items[i-start].Init(itemData[dataIndex], dataIndex);
		}
		OnValueChange (_items [middle].ItemIndex, _items [middle].ItemText,false);
	}	

	/// <summary>
	/// Updates the visible wheel items. Gets called by the OnValueChanged of the ScrollRect.
	/// Checks every update if a new wheel item is currently in the middle and moves the upper and lower items accordingly.
	/// </summary>
	public virtual void UpdateContent()
	{
		if (_items == null || _items.Count < itemCount)
			return;

		int middle = (itemCount -1) / 2;
		
		RectTransform r = _items[middle].GetComponent<RectTransform>();
		float diff = contentRect.anchoredPosition.y + r.anchoredPosition.y;

		//if the current middle item is more than half of its height above or below the middle, this means the items need to be moved, so there should be a new middle item.
		if (diff  > prefabHeight * 0.5f)
		{
			ShiftWheelItems(true);
			Select(_items[middle]);
		}
		else if (diff < -prefabHeight * 0.5f)
		{
			ShiftWheelItems(false);
			Select(_items[middle]);
		}
	}

	/// <summary>
	/// Change the order of list items and set them to the right data
	/// </summary>
	/// <param name="bottomToTop">Bottom item goes to top? (true) Or top item goes to bottom? (false)</param>
	protected void ShiftWheelItems(bool bottomToTop, bool empty = false)
	{
		int index, newIndex; 
		if (bottomToTop)
		{
			// get the index after that of the last item (the lowest) in the list of items.
			index = _items[itemCount - 1].ItemIndex;
			newIndex = getDataIndex(index + 1);

			// move the first item (the top) to the bottom of the list
			_items[0].transform.localPosition -= new Vector3(0, prefabHeight * itemCount);
			//populate it with the next value and index
			if (!empty) _items[0].Init(itemData[newIndex], newIndex);
			else _items[0].Init("", -1);
			// add it to the back of the list
			_items.Add(_items[0]);
			//remove it from the start of the list
			_items.RemoveAt(0);
		}
		else
		{
			// get the index before that of the first item (the top most) in the list of items.
			index = _items[0].ItemIndex;
			newIndex = getDataIndex(index - 1);

			// move the last item (the bottom) to the top of the list
			_items[itemCount-1].transform.localPosition += new Vector3(0, prefabHeight * itemCount);
			//populate it with the next value and index
			if (!empty) _items[itemCount - 1].Init(itemData[newIndex], newIndex);
			else _items[itemCount - 1].Init("", -1);
			// add it to the front of the list
			_items.Insert(0,_items[itemCount-1]);
			//remove it from the end of the list
			_items.RemoveAt(itemCount);
		}
	}

	public void Select(WheelItem item)
	{
			//update the current selection with the new middle
			_curSelection = item.ItemIndex;
			//call OnValueChange
			OnValueChange(item.ItemIndex, item.ItemText);
	}

	/// <summary>
	/// Gets the modulo of the given index and the length of the itemData.
	/// Is an actual modulo function and not a remainder, like %
	/// </summary>
	/// <returns>The index in itemData</returns>
	/// <param name="index">The number you want to know the index in itemData of (can be negative)</param>
	protected int getDataIndex(int index)
	{
		int dataIndex = index - itemData.Length * Mathf.FloorToInt((float)index / (float)itemData.Length);
		return dataIndex; 
	}

}