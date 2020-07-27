using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;

public class GraphUI : MonoBehaviour
{
    public UILineRenderer lineRenderer;
    public Text nameText;

    public Samples samples
    {
        get;
        set;
    }

    public Color color
    {
        get { return lineRenderer.color; }
        set { lineRenderer.color = value; }
    }

    public float minY
    {
        get { return _minY; }
        set
        {
            _minY = value;
            UpdateGraph();
        }
    }
    float _minY;

    public float maxY
    {
        get { return _maxY; }
        set
        {
            _maxY = value;
            UpdateGraph();
        }
    }
    float _maxY;

    public int timeSpanMillis
    {
        get { return _timeSpanMillis; }
        set
        {
            _timeSpanMillis = value;
            UpdateGraph();
        }
    }
    int _timeSpanMillis;

    public int thresholdTime
    {
        get
        {
            return samples.samples.Last().millis - timeSpanMillis;
        }
    }

    private void Awake()
    {
    }

    public void Setup(string axisName)
    {
        nameText.text = axisName;
    }

    Vector2 ScaleSample(float s, int t)
    {
        float x = (t - thresholdTime) * lineRenderer.rectTransform.rect.width / timeSpanMillis;
        float y = (s - minY) * lineRenderer.rectTransform.rect.height / (maxY - minY);
        return new Vector2(x, y);
    }

    public void UpdateGraph()
    {
        if (samples == null)
            return;
        if (samples.samples.Count == 0)
            return;

        // Update the list of points sent to the graph!
        var lastSample = samples.samples.Last();

        // Iterate through the samples to find the oldest sample index
        // that is younger than our time window seconds from the newest sample.
        int thresholdTime = lastSample.millis - timeSpanMillis;
        int oldestSampleIndex = samples.samples.Count - 1;
        for (; oldestSampleIndex >= 0; --oldestSampleIndex)
        {
            var sample = samples.samples[oldestSampleIndex];
            if (sample.millis < thresholdTime)
                break;
        }

        // Since we found the first sample TOO old, re-increment the
        // index once to get the last sample old ENOUGH!
        oldestSampleIndex++;
        var oldestSample = samples.samples[oldestSampleIndex];

        // Create the new array of points for rendering
        int pointCount = samples.samples.Count - oldestSampleIndex;
        var points = new Vector2[pointCount];

        for (int i = 0; i < pointCount; ++i)
        {
            var sample = samples.samples[i + oldestSampleIndex];
            points[i] = ScaleSample(sample.value, sample.millis);
        }

        lineRenderer.Points = points;
        lineRenderer.SetVerticesDirty();
    }
}
