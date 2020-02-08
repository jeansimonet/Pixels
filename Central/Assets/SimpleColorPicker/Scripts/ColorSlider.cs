using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleColorPicker.Scripts
{
	/// <summary>
	/// Slider representation (RGBA, HSV).
	/// </summary>
	public class ColorSlider : MonoBehaviour
	{
		public int MaxValue;
		public Slider Slider;
		public InputField InputField;
		public ColorPicker ColorPicker;

		/// <summary>
		/// Quick access.
		/// </summary>
		public float Value { get { return Slider.value; } }

		/// <summary>
		/// Set lider value.
		/// </summary>
		public void Set(float value)
		{
			Slider.value = value;
			InputField.text = Mathf.RoundToInt(value * MaxValue).ToString();
		}

		/// <summary>
		/// Called when slider value changed.
		/// </summary>
		public void OnValueChanged(float value)
		{
			if (ColorPicker.Locked) return;

			InputField.text = Mathf.RoundToInt(value * MaxValue).ToString();
			ColorPicker.OnSliderChanged();
		}

		/// <summary>
		/// Called when input field value changed.
		/// </summary>
		public void OnValueChanged(string value)
		{
			if (ColorPicker.Locked) return;

			value = value.Replace("-", null);

			if (value == "")
			{
				InputField.text = "";
			}
			else
			{
				var integer = Mathf.Min(int.Parse(value), MaxValue);

				InputField.text = integer.ToString();
				Slider.value = (float) integer / MaxValue;
				ColorPicker.OnSliderChanged();
			}
		}
	}
}