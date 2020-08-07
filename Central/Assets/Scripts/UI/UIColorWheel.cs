using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Based on https://gist.github.com/halgrimmur/bbe50a7bb2621f0a577524edb663669f

public class UIColorWheel : MaskableGraphic, IPointerClickHandler
{
    [Header("Parameters")]
    public int radialWedgeCount = 3;
    public int circularWedgeCount = 16;
    public float saturationPower = 1.2f;
    
    float _value = 1.0f;
    public float colorValue
    {
        get { return _value; }
        set
        {
            _value = value;
            SetVerticesDirty();
        }
    }

    public const float minAngle = 10.0f * Mathf.Deg2Rad;

    float angleInc => 2.0f * Mathf.PI / circularWedgeCount;
    float radius => Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
    float radiusInc => radius / (radialWedgeCount + 1);

    public delegate void ColorWheelClickedEvent(Color color, int hueIndex, int saturationIndex);
    public ColorWheelClickedEvent onClicked;

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
        SetMaterialDirty();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localCoords = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out localCoords))
        {
            // Extract angle and distance
            float angle = Mathf.Repeat(Mathf.Atan2(localCoords.y, localCoords.x), 2.0f * Mathf.PI);
            float radius = localCoords.magnitude;

            // From that, figure out the indices
            int j = Mathf.FloorToInt(radius / radiusInc) - 1;
            int i = Mathf.FloorToInt(angle / angleInc);

            var color = ComputeColor(i, j);
            onClicked?.Invoke(color, i, j);
        }
    }


    // if no texture is configured, use the default white texture as mainTexture
    public override Texture mainTexture
    {
        get
        {
            return s_WhiteTexture;
        }
    }
    
    // helper to easily create quads for our ui mesh. You could make any triangle-based geometry other than quads, too!
    public static void AddQuad(VertexHelper vh, Color color, Vector2 corner0, Vector2 corner1, Vector2 corner2, Vector2 corner3)
    {
        var i = vh.currentVertCount;
            
        UIVertex vert = new UIVertex();
        vert.color = color;
        vert.uv0 = new Vector2(0.5f, 0.5f);

        vert.position = corner0;
        vh.AddVert(vert);

        vert.position = corner1;
        vh.AddVert(vert);

        vert.position = corner2;
        vh.AddVert(vert);

        vert.position = corner3;
        vh.AddVert(vert);
            
        vh.AddTriangle(i+0,i+2,i+1);
        vh.AddTriangle(i+3,i+2,i+0);
    }

    // actually update our mesh
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // Clear vertex helper to reset vertices, indices etc.
        vh.Clear();

        // Center
        Vector2 center = Vector2.zero;

        // Add wedges
        int subWedgeCount = Mathf.Max(1, Mathf.CeilToInt(angleInc / minAngle));
        float subWedgeAngleInc = 2.0f * Mathf.PI / (circularWedgeCount * subWedgeCount);
        for (int j = 0; j < radialWedgeCount; ++j)
        {
            float prevRadius = radiusInc * (j + 1);
            float nextRadius = radiusInc * (j + 2);
            for (int i = 0; i < circularWedgeCount; ++i)
            {
                Color wedgeColor = ComputeColor(i, j);

                // A wedge has the same color, but is subdivided into subwedges for more rounded look
                for (int k = 0; k < subWedgeCount; ++k)
                {
                    float prevAngle = subWedgeAngleInc * (i * subWedgeCount + k);
                    float nextAngle = subWedgeAngleInc * (i * subWedgeCount + k + 1);

                    Vector2 corner0 = center + new Vector2(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle)) * nextRadius;
                    Vector2 corner1 = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * nextRadius;
                    Vector2 corner2 = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * prevRadius;
                    Vector2 corner3 = center + new Vector2(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle)) * prevRadius;

                    AddQuad(vh, wedgeColor, corner0, corner1, corner2, corner3);
                }
            }
        }
    }


    public Color ComputeColor(int wedgeHueIndex, int wedgeSatIndex)
    {
        float hue = Mathf.Repeat(((float)wedgeHueIndex + 0.5f) / circularWedgeCount, 1.0f);
        float sat = Mathf.Pow((float)(wedgeSatIndex + 1) / radialWedgeCount, saturationPower);
        return Color.HSVToRGB(hue, sat, colorValue);
    }

    public bool FindColor(Color color, out int hueIndex, out int satIndex)
    {
        hueIndex = -1;
        satIndex = -1;

        float hue, sat, val;
        Color.RGBToHSV(color, out hue, out sat, out val);

        const float valueEpsilon = 0.01f;
        bool ret = Mathf.Abs(val - colorValue) < valueEpsilon;
        if (ret)
        {
            // The value matches, look for the hue and lightness values
            float satIndexF = Mathf.Pow(sat, 1.0f / saturationPower) * radialWedgeCount - 1.0f;
            satIndex = Mathf.RoundToInt(satIndexF);
            const float satEpsilon = 0.05f;
            ret = Mathf.Abs(satIndexF - satIndex) < satEpsilon;
            if (ret)
            {
                float hueIndexF = hue * circularWedgeCount - 0.5f;
                hueIndex = Mathf.RoundToInt(hueIndexF);
                float hueEpsilon = (1.0f - sat) * 0.05f + 0.05f;
                ret = Mathf.Abs(hueIndexF - hueIndex) < hueEpsilon;
            }
        }
        return ret;
    }

}