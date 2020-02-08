using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.SimpleColorPicker.Scripts
{
	/// <summary>
	/// Main gradient representation (brightness/saturation).
	/// </summary>
	public class PaletteGradient : MonoBehaviour, IPointerDownHandler, IDragHandler
	{
		public ColorJoystick ColorJoystick;

		/// <summary>
		/// Called when user clicks area.
		/// </summary>
		public void OnPointerDown(PointerEventData eventData)
		{
			Vector2 position;

			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, Camera.main, out position))
			{
				return;
			}

			ColorJoystick.OnDrag(eventData);
		}

		/// <summary>
		/// Called when user drags color picker.
		/// </summary>
		public void OnDrag(PointerEventData eventData)
		{
			ColorJoystick.OnDrag(eventData);
		}
	}
}