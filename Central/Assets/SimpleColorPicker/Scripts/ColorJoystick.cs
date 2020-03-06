using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.SimpleColorPicker.Scripts
{
	/// <summary>
	/// Color picker 'joystick' representation.
	/// </summary>
	public class ColorJoystick : MonoBehaviour, IDragHandler
	{
		public Image Center;
		public RectTransform RectTransform;
		public ColorPicker ColorPicker;

		/// <summary>
		/// Called when picker moved.
		/// </summary>
		public void OnDrag(PointerEventData eventData)
		{
			Vector2 position;

			if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, eventData.position, Camera.main, out position))
			{
				return;
			}

			position.x = Mathf.Max(position.x, RectTransform.rect.min.x);
			position.y = Mathf.Max(position.y, RectTransform.rect.min.y);
			position.x = Mathf.Min(position.x, RectTransform.rect.max.x);
			position.y = Mathf.Min(position.y, RectTransform.rect.max.y);
			transform.localPosition = position;

			var texture = ColorPicker.Texture;
			var x = position.x / RectTransform.rect.width * texture.width;
			var y = position.y / RectTransform.rect.height * texture.height;

			float h, s, v;

			Color.RGBToHSV(ColorPicker.Color, out h, out s, out v);

			var color = Color.HSVToRGB(ColorPicker.H.Value, x / texture.width,  y / texture.height);

			ColorPicker.SetColor(color, picker: false);
		}
	}
}