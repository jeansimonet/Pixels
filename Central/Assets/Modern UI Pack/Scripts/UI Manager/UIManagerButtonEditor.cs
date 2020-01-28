using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
namespace Michsky.UI.ModernUIPack
{
    [CustomEditor(typeof(UIManagerButton))]
    public class UIManagerButtonEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var UIManagerButton = target as UIManagerButton;

            EditorGUI.indentLevel++;

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.BASIC)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.basicFilled = EditorGUILayout.ObjectField("Background", UIManagerButton.basicFilled, typeof(Image), true) as Image;
                    UIManagerButton.basicText = EditorGUILayout.ObjectField("Text", UIManagerButton.basicText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.BASIC_ONLY_ICON)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.basicOnlyIconFilled = EditorGUILayout.ObjectField("Background", UIManagerButton.basicOnlyIconFilled, typeof(Image), true) as Image;
                    UIManagerButton.basicOnlyIconIcon = EditorGUILayout.ObjectField("Icon", UIManagerButton.basicOnlyIconIcon, typeof(Image), true) as Image;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.BASIC_WITH_ICON)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.basicWithIconFilled = EditorGUILayout.ObjectField("Background", UIManagerButton.basicWithIconFilled, typeof(Image), true) as Image;
                    UIManagerButton.basicWithIconIcon = EditorGUILayout.ObjectField("Icon", UIManagerButton.basicWithIconIcon, typeof(Image), true) as Image;
                    UIManagerButton.basicWithIconText = EditorGUILayout.ObjectField("Text", UIManagerButton.basicWithIconText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.BASIC_OUTLINE)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.basicOutlineBorder = EditorGUILayout.ObjectField("Border", UIManagerButton.basicOutlineBorder, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineFilled = EditorGUILayout.ObjectField("Filled", UIManagerButton.basicOutlineFilled, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineText = EditorGUILayout.ObjectField("Text", UIManagerButton.basicOutlineText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                    UIManagerButton.basicOutlineTextHighligted = EditorGUILayout.ObjectField("Text Highlighted", UIManagerButton.basicOutlineTextHighligted, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.BASIC_OUTLINE_ONLY_ICON)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.basicOutlineOOBorder = EditorGUILayout.ObjectField("Border", UIManagerButton.basicOutlineOOBorder, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineOOFilled = EditorGUILayout.ObjectField("Filled", UIManagerButton.basicOutlineOOFilled, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineOOIcon = EditorGUILayout.ObjectField("Icon", UIManagerButton.basicOutlineOOIcon, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineOOIconHighlighted = EditorGUILayout.ObjectField("Icon Highlighted", UIManagerButton.basicOutlineOOIconHighlighted, typeof(Image), true) as Image;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.BASIC_OUTLINE_WITH_ICON)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.basicOutlineWOBorder = EditorGUILayout.ObjectField("Border", UIManagerButton.basicOutlineWOBorder, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineWOFilled = EditorGUILayout.ObjectField("Filled", UIManagerButton.basicOutlineWOFilled, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineWOIcon = EditorGUILayout.ObjectField("Icon", UIManagerButton.basicOutlineWOIcon, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineWOIconHighlighted = EditorGUILayout.ObjectField("Icon Highlighted", UIManagerButton.basicOutlineWOIconHighlighted, typeof(Image), true) as Image;
                    UIManagerButton.basicOutlineWOText = EditorGUILayout.ObjectField("Text", UIManagerButton.basicOutlineWOText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                    UIManagerButton.basicOutlineWOTextHighligted = EditorGUILayout.ObjectField("Text Highlighted", UIManagerButton.basicOutlineWOTextHighligted, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.RADIAL_ONLY_ICON)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.radialOOBackground = EditorGUILayout.ObjectField("Background", UIManagerButton.radialOOBackground, typeof(Image), true) as Image;
                    UIManagerButton.radialOOIcon = EditorGUILayout.ObjectField("Icon", UIManagerButton.radialOOIcon, typeof(Image), true) as Image;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.RADIAL_OUTLINE_ONLY_ICON)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.radialOutlineOOBorder = EditorGUILayout.ObjectField("Border", UIManagerButton.radialOutlineOOBorder, typeof(Image), true) as Image;
                    UIManagerButton.radialOutlineOOFilled = EditorGUILayout.ObjectField("Filled", UIManagerButton.radialOutlineOOFilled, typeof(Image), true) as Image;
                    UIManagerButton.radialOutlineOOIcon = EditorGUILayout.ObjectField("Icon", UIManagerButton.radialOutlineOOIcon, typeof(Image), true) as Image;
                    UIManagerButton.radialOutlineOOIconHighlighted = EditorGUILayout.ObjectField("Icon Highlighted", UIManagerButton.radialOutlineOOIconHighlighted, typeof(Image), true) as Image;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.ROUNDED)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.roundedBackground = EditorGUILayout.ObjectField("Background", UIManagerButton.roundedBackground, typeof(Image), true) as Image;
                    UIManagerButton.roundedText = EditorGUILayout.ObjectField("Text", UIManagerButton.roundedText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                }
            }

            using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(UIManagerButton.buttonType == UIManagerButton.ButtonType.ROUNDED_OUTLINE)))
            {
                if (group.visible == true)
                {
                    UIManagerButton.roundedOutlineBorder = EditorGUILayout.ObjectField("Border", UIManagerButton.roundedOutlineBorder, typeof(Image), true) as Image;
                    UIManagerButton.roundedOutlineFilled = EditorGUILayout.ObjectField("Filled", UIManagerButton.roundedOutlineFilled, typeof(Image), true) as Image;
                    UIManagerButton.roundedOutlineText = EditorGUILayout.ObjectField("Text", UIManagerButton.roundedOutlineText, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                    UIManagerButton.roundedOutlineTextHighligted = EditorGUILayout.ObjectField("Text Highlighted", UIManagerButton.roundedOutlineTextHighligted, typeof(TextMeshProUGUI), true) as TextMeshProUGUI;
                }
            }
        }
    }
}
#endif