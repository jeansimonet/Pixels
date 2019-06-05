using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BorderHandle : MonoBehaviour, IFocusable, IPointerDownHandler, IDragHandler
{
	public enum BorderSide { Left, Right }

	[SerializeField]
	BorderSide _side = default(BorderSide);

	public event System.Action Moved;

	public BorderSide Side { get { return _side; } }
	public bool HasFocus { get; private set; }

	Vector2 _dragOffset;

	public void GiveFocus()
	{
		HasFocus = true;
		GetComponentsInParent<IFocusable>().First(f => (object)f != this).GiveFocus();
		var anim = GetComponentInParent<TimelineView>().ActiveColorAnimator;
		if (anim != null) anim.ColorSlider.SelectHandle(null); //TODO 
	}

	public void RemoveFocus()
	{
		HasFocus = false;
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		_dragOffset = transform.position - Input.mousePosition;
		GiveFocus();
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		var parentRect = (transform.parent as RectTransform).rect;
		float xMin = transform.parent.TransformPoint(parentRect.xMin, 0, 0).x;
		float xMax = transform.parent.TransformPoint(parentRect.xMax, 0, 0).x;

		float x = Mathf.Clamp(Input.mousePosition.x + _dragOffset.x, xMin, xMax);
		transform.position = new Vector2(x, transform.position.y);

		if (Moved != null) Moved();
	}

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{
	}
}
