using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RotationSlider : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public float dragSpeed = 10.0f;
    public float dragTiltSpeed = 5.0f;


    Vector3 dragStart;
    float dragStartRotation;
    float dragStartTilt;
    SingleDiceRenderer diceRenderer;

    public void Setup(SingleDiceRenderer diceRenderer)
    {
        this.diceRenderer = diceRenderer;
    }

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		EventSystem.current.SetSelectedGameObject(gameObject);
	}

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
		dragStart = Input.mousePosition;
        dragStartRotation = diceRenderer.die.currentAngle;
        dragStartTilt = diceRenderer.tilt;
    }

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
        var dragDelta = Input.mousePosition - dragStart;
        diceRenderer.die.SetCurrentAngle(dragStartRotation - dragDelta.x * dragSpeed);
        float newTilt = Mathf.Repeat(dragStartTilt - dragDelta.y * dragTiltSpeed + 180.0f, 360.0f) - 180.0f;
        diceRenderer.tilt = Mathf.Clamp(newTilt, -90.0f, 90.0f);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        // Nothing really
        diceRenderer.die.SetAuto(false);
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
