using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static MultiSlider;

public class MultiSliderHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
	public RectTransform buttonsRoot;
	public Image selectionImage;
	public Image colorImage;
	Canvas _canvas = null;
	MultiSlider _slider;
	Vector2 _dragOffset;

	public bool Selected => _slider.ActiveHandle == this;
	public Color Color => colorImage.color;

	public void ChangeColor(Color color)
	{
		ChangeColor(color, false);
	}

	public void ChangeColor(Color color, bool noRepaint)
	{
		colorImage.color = color;
		if (!noRepaint)
		{
			_slider.Repaint();
		}
	}

	public MultiSliderHandle Duplicate()
	{
		var dupHandle = GameObject.Instantiate<MultiSliderHandle>(this, transform.parent);
		_slider.Repaint(); //TODO slider should be notified instead
		_slider.SelectHandle(dupHandle);
		return dupHandle;
	}

	public void DuplicateSelf()
	{
		var dupHandle = Duplicate();
		// Move the new handle, but clamp it intelligently
		var rect = (_slider.transform as RectTransform).rect;
		Vector2 min = _slider.transform.TransformPoint(rect.xMin, rect.yMin, 0);
		Vector2 max = _slider.transform.TransformPoint(rect.xMax, rect.yMax, 0);
		float width = max.x - min.x;

		float newX = transform.position.x;
		if (newX >= max.x - width * 0.1f)
		{
			newX -= width * 0.1f;
		}
		else
		{
			newX += width * 0.1f;
		}
		dupHandle.transform.position = new Vector2(newX, transform.position.y);
	}

	public void RemoveSelf()
	{
		_slider.RemoveHandle(this);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		_dragOffset = transform.position - Input.mousePosition;

		EventSystem.current.SetSelectedGameObject(gameObject);
		_slider.SelectHandle(this);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		var rect = (_slider.transform as RectTransform).rect;
		Vector2 min = _slider.transform.TransformPoint(rect.xMin, rect.yMin, 0);
		Vector2 max = _slider.transform.TransformPoint(rect.xMax, rect.yMax, 0);

		if (_slider.Direction == SliderDirection.Horizontal)
		{
			float x = Mathf.Clamp(Input.mousePosition.x + _dragOffset.x, min.x, max.x);
			transform.position = new Vector2(x, transform.position.y);
		}
		else
		{
			float y = Mathf.Clamp(Input.mousePosition.y + _dragOffset.y, min.y, max.y);
            transform.position = new Vector2(transform.position.x, y);
		}

		Snap();

		_slider.Repaint();
	}

	void OnHandleSelected(MultiSliderHandle handle)
	{
		selectionImage.color = (handle == this) ? Color.white : new Color(1.0f, 1.0f, 1.0f, 0.25f);
		buttonsRoot.gameObject.SetActive(handle == this);
		Repaint();
	}

	void Snap()
	{
		// float unit = GetComponentInParent<TimelineView>().Unit; //TODO
		// float snap = GetComponentInParent<TimelineView>().SnapInterval * unit;
		float snap = 0.1f;

		var rect = (_slider.transform as RectTransform).rect;
		Vector2 min = _slider.transform.TransformPoint(rect.xMin, rect.yMin, 0);
		Vector2 max = _slider.transform.TransformPoint(rect.xMax, rect.yMax, 0);

		if (_slider.Direction == SliderDirection.Horizontal)
		{
            float x = min.x + snap * Mathf.RoundToInt((transform.position.x - min.x) / snap);
            float y = Mathf.Lerp(min.y, max.y, _slider.HandlePosition);
			transform.position = new Vector2(x, y);
		}
		else
		{
			float x = Mathf.Lerp(min.x, max.x, _slider.HandlePosition);
            float y = min.y + snap * Mathf.RoundToInt((transform.position.y - min.y) / snap);
            transform.position = new Vector2(x, y);
		}
	}

	void Repaint()
	{
		//transform.localScale = Vector2.one * (Selected ? 1.2f : 1f);
		if (_canvas != null)
		{
			_canvas.overrideSorting = Selected;
		}
	}

	void OnEnable()
	{
		_slider = GetComponentInParent<MultiSlider>();
		_slider.HandleSelected += OnHandleSelected;
	}

	void OnDisable()
	{
		_slider.HandleSelected -= OnHandleSelected;
		_slider = null;
	}

	void Awake()
	{
		_canvas = GetComponent<Canvas>();
		buttonsRoot.gameObject.SetActive(false);
	}
}
