using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorWheelSelection : MaskableGraphic
{
    [Header("Parameters")]
    public float selectionThickness = 5.0f;

    public UIColorWheel colorWheel => GetComponentInParent<UIColorWheel>();
    public int radialWedgeCount => colorWheel.radialWedgeCount;
    public int circularWegdeCount => colorWheel.circularWedgeCount;

    float angleInc => 2.0f * Mathf.PI / circularWegdeCount;
    float radius => Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) * 0.5f;
    float radiusInc => radius / (radialWedgeCount + 1);

    int _selectedHueIndex = -1;
    int _selectedSatIndex = -1;
    public int selectedHueIndex
    {
        get { return _selectedHueIndex; }
        set
        {
            _selectedHueIndex = value;
            SetVerticesDirty();
        }
    }
    public int selectedSatIndex
    {
        get { return _selectedSatIndex; }
        set
        {
            _selectedSatIndex = value;
            SetVerticesDirty();
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        SetVerticesDirty();
    }

    // actually update our mesh
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        // Clear vertex helper to reset vertices, indices etc.
        vh.Clear();

        if (selectedHueIndex >= 0 && selectedSatIndex >= 0 && selectedSatIndex < colorWheel.radialWedgeCount)
        {
            // Center
            Vector2 center = Vector2.zero;

            // Add wedges
            int subWedgeCount = Mathf.Max(1, Mathf.CeilToInt(angleInc / UIColorWheel.minAngle));
            float subWedgeAngleInc = 2.0f * Mathf.PI / (circularWegdeCount * subWedgeCount);

            float prevRadius = radiusInc * (selectedSatIndex + 1);
            float nextRadius = radiusInc * (selectedSatIndex + 2);

            float borderPrevRadius = prevRadius - selectionThickness;
            float borderNextRadius = nextRadius + selectionThickness;

            // A wedge has the same color, but is subdivided into subwedges for more rounded look
            for (int k = 0; k < subWedgeCount; ++k)
            {
                float prevAngle = subWedgeAngleInc * (selectedHueIndex * subWedgeCount + k);
                float nextAngle = subWedgeAngleInc * (selectedHueIndex * subWedgeCount + k + 1);

                Vector2 corner00 = center + new Vector2(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle)) * prevRadius;
                Vector2 corner01 = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * prevRadius;
                Vector2 corner02 = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * borderPrevRadius;
                Vector2 corner03 = center + new Vector2(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle)) * borderPrevRadius;
                UIColorWheel.AddQuad(vh, this.color, corner00, corner01, corner02, corner03);

                Vector2 corner10 = center + new Vector2(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle)) * borderNextRadius;
                Vector2 corner11 = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * borderNextRadius;
                Vector2 corner12 = center + new Vector2(Mathf.Cos(nextAngle), Mathf.Sin(nextAngle)) * nextRadius;
                Vector2 corner13 = center + new Vector2(Mathf.Cos(prevAngle), Mathf.Sin(prevAngle)) * nextRadius;
                UIColorWheel.AddQuad(vh, this.color, corner10, corner11, corner12, corner13);
            }

            // Add sides
            float minAngle = subWedgeAngleInc * (selectedHueIndex * subWedgeCount);
            Vector2 outwardVec = new Vector2(Mathf.Cos(minAngle), Mathf.Sin(minAngle));
            Vector2 rightVec = new Vector2(outwardVec.y, -outwardVec.x) * selectionThickness;

            Vector2 corner20 = center + outwardVec * borderNextRadius + rightVec;
            Vector2 corner21 = center + outwardVec * borderNextRadius;
            Vector2 corner22 = center + outwardVec * borderPrevRadius;
            Vector2 corner23 = center + outwardVec * borderPrevRadius + rightVec;
            UIColorWheel.AddQuad(vh, this.color, corner20, corner21, corner22, corner23);

            float maxAngle = subWedgeAngleInc * (selectedHueIndex * subWedgeCount + subWedgeCount);
            Vector2 outward2Vec = new Vector2(Mathf.Cos(maxAngle), Mathf.Sin(maxAngle));
            Vector2 leftVec = new Vector2(-outward2Vec.y, outward2Vec.x) * selectionThickness;

            Vector2 corner30 = center + outward2Vec * borderNextRadius;
            Vector2 corner31 = center + outward2Vec * borderNextRadius + leftVec;
            Vector2 corner32 = center + outward2Vec * borderPrevRadius + leftVec;
            Vector2 corner33 = center + outward2Vec * borderPrevRadius;
            UIColorWheel.AddQuad(vh, this.color, corner30, corner31, corner32, corner33);
        }
    }
}
