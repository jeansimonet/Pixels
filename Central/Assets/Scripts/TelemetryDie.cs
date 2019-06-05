using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public struct Sample
{
    public int millis;
    public float value;
}

public class Samples
{
    public List<Sample> samples = new List<Sample>();
}

class GraphInstance
{
    public string name;
    public GraphUI instance;
    public Samples samples;
    public System.Func<Vector3, float, float> extractValueFunc;
}

public class TelemetryDie : MonoBehaviour
{
    public const int MaxPoints = 1000;
    public const int GraphMaxTime = 5000; // millis

    List<GraphInstance> graphs;
    int lastSampleTime;

    void Awake()
    {
        int childCount = transform.childCount;
        for (int i = 1; i < childCount; ++i)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }

    void Start()
    {
    }

    public void Setup(string name)
    {
        gameObject.name = name;

        for (int i = 1; i < transform.childCount; ++i)
        {
            GameObject.Destroy(transform.GetChild(i).gameObject);
        }
        graphs = new List<GraphInstance>();

        // Graph the magnitude and euler angles
        AddGraph((acc, dt) => acc.x, -4, 4, Color.red, "X");
        AddGraph((acc, dt) => acc.y, -4, 4, Color.green, "Y");
        AddGraph((acc, dt) => acc.z, -4, 4, Color.blue, "Z");
        //AddGraph((acc, dt) => acc.magnitude, -4, 4, Color.yellow, "Mag");
    }

    public void Clear()
    {
        if (graphs != null)
        {
            foreach (var graph in graphs)
            {
                graph.samples.samples.Clear();
            }
        }
    }

    public void OnTelemetryReceived(Vector3 acc, int millis)
    {
        float deltaTime = (float)(millis - lastSampleTime) / 1000.0f;
        // Update the graphs
        foreach (var graph in graphs)
        {
            float val = graph.extractValueFunc(acc, deltaTime);
            graph.samples.samples.Add(new Sample() { millis = millis, value = val });
            graph.instance.UpdateGraph();
        }
        lastSampleTime = millis;
    }

    public void SaveToFile(string namePrefix)
    {
        int sampleRate = 8000; //8kHz
        float wavScale = 10.0f;

        foreach (var graph in graphs)
        {
            // How many samples do we want to use?
            var vals = graph.samples.samples;
            int start = vals.First().millis;
            int end = vals.Last().millis;
            int millis = end - start;
            int sampleCount = (millis + 999) * sampleRate / 1000; // To ensure rounding up!

            // Create our audio data
            float[] samples = new float[sampleCount];
            for (int i = 0; i < vals.Count - 1; ++i)
            {
                // Figure out where to place the sample
                int indexStart = (vals[i].millis - start) * sampleRate / 1000;
                int indexEnd = (vals[i + 1].millis - start) * sampleRate / 1000;
                for (int j = indexStart; j < indexEnd; ++j)
                {
                    float pct = (float)(j - indexStart) / (indexEnd - indexStart);
                    samples[j] = Mathf.Lerp(vals[i].value, vals[i + 1].value, pct) / wavScale;
                }
            }

            // Last sample
            int indexLast = (vals.Last().millis - start) * sampleRate / 1000;
            for (int j = indexLast; j < samples.Length; ++j)
            {
                samples[j] = vals.Last().value / wavScale;
            }

            // Create the audio clip
            var clip = AudioClip.Create(graph.name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            SavWav.Save(namePrefix + "_" + graph.name, clip);
        }
    }

    void AddGraph(System.Func<Vector3, float, float> func, float minY, float maxY, Color color, string graphName)
    {
        // Create the samples
        var samples = new Samples();

        var template = transform.GetChild(0).gameObject;
        var go = template;
        if (graphs.Count > 0)
        {
            // Copy first item rather than use it
            go = GameObject.Instantiate(template, transform);
        }

        var ui = go.GetComponent<GraphUI>();
        var rectX = go.GetComponent<RectTransform>();
        rectX.offsetMax = new Vector2(0.0f, 0.0f);
        rectX.offsetMin = new Vector2(0.0f, 0.0f);

        // set the scales
        ui.minY = minY;
        ui.maxY = maxY;
        ui.timeSpanMillis = GraphMaxTime;
        ui.color = color;
        ui.samples = samples;
        ui.Setup(graphName);

        // And store it
        graphs.Add(new GraphInstance()
        {
            name = graphName,
            instance = ui,
            samples = samples,
            extractValueFunc = func
        });
    }

}
