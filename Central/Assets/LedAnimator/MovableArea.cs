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
		bool moveStretch = GetComponentInParent<TimelineView>().IsMoveStretchOn;
		float offset;
		if (moveStretch)
		{
			offset = LeftBound - _movable.transform.localPosition.x;
			_movable.transform.localPosition += offset * Vector3.right;
		}
		else
		{
			offset = LeftBound - _movable.offsetMin.x;
			_movable.offsetMin += offset * Vector2.right;
		}

		//TODO
		GetComponentInChildren<MultiSlider>().LeftBoundChanged(offset, moveStretch);
		if (moveStretch)
		{
			MoveRightHandle(offset);
		}
	}

	void OnRightHandleMoved()
	{
		float offset = RightBound - _movable.offsetMax.x;
		_movable.offsetMax += offset * Vector2.right;

		//TODO
		bool moveStretch = GetComponentInParent<TimelineView>().IsMoveStretchOn;
		GetComponentInChildren<MultiSlider>().RightBoundChanged(offset, moveStretch);
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
