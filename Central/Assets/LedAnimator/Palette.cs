using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayout))]
public class Palette : SingletonMonoBehaviour<Palette>
{
	[SerializeField]
	Sprite _sourceSprite = null;
	[SerializeField]
	Transform _autoRoot = null;
	[SerializeField]
	Toggle _specialColor = null;

	public event System.Action<Color> ColorSelected;
	public Color ActiveColor { get; private set; }

	ToggleGroup _group;

#if UNITY_EDITOR
	[ContextMenu("Generate Colors")]
	void EditorRegenerate()
	{
		if (_autoRoot.transform.childCount == 0)
		{
			Debug.LogError("Leave at least one child element which will be used as a template to generate all other items");
			return;
		}

		var grid = _autoRoot.GetComponent<GridLayoutGroup>();
		if (grid == null)
		{
			Debug.LogError("A Grid Layout is required");
			return;
		}

		if (_sourceSprite == null)
		{
			Debug.LogError("Assign a source sprite to generate Palette");
			return;
		}

		int numItemsX = _sourceSprite.texture.width;
		int numItemsY = _sourceSprite.texture.height;
		if ((numItemsX < 1) || (numItemsY < 1))
		{
			Debug.LogError("Assign a source sprite with at least one pixel");
			return;
		}
		if ((numItemsX * numItemsY) > 128)
		{
			Debug.LogError("Too many pixels in source sprite (> 128)");
			return;
		}

		var pixels = _sourceSprite.texture.GetPixels();
		if (pixels == null)
		{
			Debug.LogError("Can't read pixels from source sprite");
			return;
		}

		// Use first item as template
		var itemTransf = _autoRoot.transform.GetChild(0);
		var itemSel = itemTransf.GetComponentInChildren<Selectable>();
		if (itemSel == null)
		{
			Debug.LogError("First child must be a Selectable, it will be used as a template to generate all other items");
			return;
		}

		Debug.LogFormat("Generating {0}x{1} palette", numItemsX, numItemsY);

		Undo.SetCurrentGroupName("Regenerate palette");
		int group = Undo.GetCurrentGroup();

		Undo.RecordObject(_autoRoot.gameObject, "Regenerate palette");

		for (int i = _autoRoot.transform.childCount - 1; i > 0; --i)
		{
			Undo.DestroyObjectImmediate(_autoRoot.transform.GetChild(i).gameObject);
		}

		var rectTransf = (RectTransform)_autoRoot.transform;

		grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		grid.constraintCount = numItemsX;
		grid.cellSize = new Vector2(
			(rectTransf.rect.width - (numItemsX - 1) * grid.spacing.x - grid.padding.horizontal) / numItemsX,
			(rectTransf.rect.height - (numItemsY - 1) * grid.spacing.y - grid.padding.vertical) / numItemsY);

		// Apply color on original item
		var colors = itemSel.colors;
		colors.normalColor = pixels[numItemsX * (numItemsY - 1)];
		colors.highlightedColor = colors.normalColor;
		colors.pressedColor = colors.normalColor;
		itemSel.colors = colors;

		bool first = true;
		for (int y = (numItemsY - 1); y >= 0; --y)
		{
			int xStart = first ? 1 : 0;
			for (int x = xStart; x < numItemsX; ++x)
			{
				first = false;

				var go = GameObject.Instantiate(itemTransf.gameObject, _autoRoot.transform);
				var sel = go.GetComponentInChildren<Selectable>();
				colors.normalColor = pixels[x + y * numItemsX];
				colors.highlightedColor = colors.normalColor;
				colors.pressedColor = colors.normalColor;
				colors.selectedColor = colors.normalColor;
				sel.colors = colors;

				Undo.RegisterCreatedObjectUndo(go, "Duplicated palette item");
			}
		}

		Undo.CollapseUndoOperations(group);
	}
#endif

	public void PickColor(Color color)
	{
		if (color != ActiveColor)
		{
			ActiveColor = color;
			ColorSelected?.Invoke(ActiveColor);
		}
	}

	public void SelectColor(Color color)
	{
		bool found = false;
		foreach (var toggle in _autoRoot.GetComponentsInChildren<Toggle>())
		{
			if (toggle.colors.normalColor == color)
			{
				toggle.isOn = true;
				found = true;
				break;
			}
		}
		if (!found)
		{
			_specialColor.isOn = true;
		}
	}

	// Use this for initialization
	void Start()
	{
		_group = GetComponentInChildren<ToggleGroup>();
	}

	// Update is called once per frame
	void Update()
	{
		var e = _group.ActiveToggles().GetEnumerator();
		if (e.MoveNext())
		{
			var toggle = e.Current;
			PickColor(toggle == _specialColor ? new Color() :  toggle.colors.normalColor);
		}
	}
}
