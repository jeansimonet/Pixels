using UnityEngine;
using UnityEngine.UI;

namespace Assets.SimpleColorPicker.Scripts
{
	/// <summary>
	/// Generates rainbow for Hue vertical slider.
	/// </summary>
	public class RainbowGenerator : MonoBehaviour
	{
		public void Start ()
		{
			var texture = new Texture2D(1, 128);

			for (var i = 0; i < texture.height; i++)
			{
				texture.SetPixel(0, i, Color.HSVToRGB((float) i / (texture.height - 1), 1f, 1f));
			}

			texture.Apply();
			GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
		}
	}
}