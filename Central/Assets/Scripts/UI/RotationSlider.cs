using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RotationSlider : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float dragSpeed = 10.0f;

	Vector3 dragStart;
    float dragStartRotation;
    DiceRendererDice renderedDie;

    public void Setup(DiceRendererDice die)
    {
        renderedDie = die;
    }

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(gameObject);
	}

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
		dragStart = Input.mousePosition;
        dragStartRotation = renderedDie.currentAngle;
        Debug.Log("Begin Drag");
    }

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
        var dragDelta = Input.mousePosition - dragStart;
        renderedDie.SetCurrentAngle(dragStartRotation - dragDelta.x * dragSpeed);
        Debug.Log("Drag");
	}

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        // Nothing really
        Debug.Log("End Drag");
        renderedDie.SetAuto(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
