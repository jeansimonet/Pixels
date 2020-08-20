using UnityEngine;
using System.Collections;

public class FiniWheel : InfiniWheel {

	public override void SetData(string[] choices)
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
			int dataIndex = _curSelection + i;
			if(dataIndex >= 0 && dataIndex < choices.Length)
				_items[i-start].Init(itemData[dataIndex], dataIndex);
			else
				_items[i-start].Init("", -1);
		}
	}

	/// <summary>
	/// Select the specified index.
	/// </summary>
	/// <param name="index">Index</param>
	public override void Select (int index) {	
		if (itemData == null || _items == null)
			return;

		int start = -(itemCount-1) / 2;
		int end = (itemCount +1) / 2;
		int middle = (itemCount -1) / 2;
		_curSelection = getDataIndex(index);
		for (int i = start; i < end; i++)
		{
			int dataIndex = _curSelection + i;
			if(dataIndex >= 0 && dataIndex < itemData.Length)
				_items[i-start].Init(itemData[dataIndex], dataIndex);
			else
				_items[i-start].Init("", -1);
		}
		OnValueChange (_items [middle].ItemIndex, _items [middle].ItemText,false);
	}	

	public override void UpdateContent()
	{
		if (_items == null || _items.Count < itemCount)
			return;

		int middle = (itemCount -1) / 2;
		
		RectTransform r = _items[middle].GetComponent<RectTransform>();
		float diff = contentRect.anchoredPosition.y + r.anchoredPosition.y;

		//if the current middle item is more than half of its height above or below the middle, this means the items need to be moved, so there should be a new middle item.
		if (diff  > prefabHeight * 0.5f) // we moved down
		{
			int contentID = getDataIndex(_items[middle].ItemIndex);
			if (contentID != itemData.Length - 1)
			{
				ShiftWheelItems(true, !(contentID < itemData.Length - middle - 1));
				Select(_items[middle]);
			}
		}
		else if (diff < -prefabHeight * 0.5f) // we moved up
		{
			int contentID = getDataIndex(_items[middle].ItemIndex);
			if (contentID != 0)
			{
				ShiftWheelItems(false, !(contentID > middle));
				Select(_items[middle]);
			}
		}
	}
}
