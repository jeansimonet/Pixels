using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableArea : MonoBehaviour
{
	[SerializeField]
	BorderHandle _leftHandle = null;
	[SerializeField]
	BorderHandle _rightHandle = null;
	[SerializeField]
	RectTransform _movable = null;

	public float LeftBound
	{
		get { return _leftHandle.transform.localPosition.x; }
		set { MoveLeftHandle(value - _leftHandle.transform.localPosition.x); }
	}

	public float RightBound
	{
		get { return _rightHandle.transform.localPosition.x; }
		set { MoveRightHandle(value - _rightHandle.transform.localPosition.x); }
	}

	public RectTransform Movable => _movable;

	public void Maximize()
	{
		var pos = _rightHandle.transform.localPosition;
		pos.x = (transform as RectTransform).rect.width;
		_rightHandle.transform.localPosition = pos;

		OnLeftHandleMoved();
		OnRightHandleMoved();
	}

	void MoveLeftHandle(float offset)
	{
		_leftHandle.transform.localPosition += offset * Vector3.right;
		OnLeftHandleMoved();
	}

	void MoveRightHandle(float offset)
	{
		_rightHandle.transform.localPosition += offset * Vector3.right;
		OnRightHandleMoved();
	}

	void OnLeftHandleMoved()
	{
		float offset = LeftBound - _movable.offsetMin.x;
		_movable.offsetMin += offset * Vector2.right;

		//TODO
		bool stretch = GetComponentInParent<TimelineView>().IsMoveStretchOn;
		GetComponentInChildren<MultiSlider>().LeftBoundChanged(offset, stretch);
		if (stretch)
		{
			MoveRightHandle(offset);
		}
	}

	void OnRightHandleMoved()
	{
		float offset = RightBound - _movable.offsetMax.x;
		_movable.offsetMax += offset * Vector2.right;

		//TODO
		bool stretch = GetComponentInParent<TimelineView>().IsMoveStretchOn;
		GetComponentInChildren<MultiSlider>().RightBoundChanged(offset, stretch);
	}

	void OnEnable()
	{
		_leftHandle.Moved += OnLeftHandleMoved;
		_rightHandle.Moved += OnRightHandleMoved;
	}

	void OnDisable()
	{
		_leftHandle.Moved -= OnLeftHandleMoved;
		_rightHandle.Moved -= OnRightHandleMoved;
	}

	// Use this for initialization
	void Start()
	{
		OnLeftHandleMoved();
		OnRightHandleMoved();
	}
}
