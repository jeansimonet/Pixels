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
    public System.Func<AccelFrame, float> extractValueFunc;
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
        AddGraph(frame => frame.acc.x, -4, 4, Color.red, "AccX");
        AddGraph(frame => frame.acc.y, -4, 4, Color.green, "AccY");
        AddGraph(frame => frame.acc.z, -4, 4, Color.blue, "AccZ");
        AddGraph(frame => frame.jerk.x, -1, 1, Color.red, "JerkX");
        AddGraph(frame => frame.jerk.y, -1, 1, Color.green, "JerkY");
        AddGraph(frame => frame.jerk.z, -1, 1, Color.blue, "JerkZ");
        AddGraph(frame => frame.slowSigma, -1, 1, Color.yellow, "SlowSigma");
        AddGraph(frame => frame.fastSigma, -1, 1, Color.cyan, "FastSigma");
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

    public void OnTelemetryReceived(AccelFrame frame)
    {
        // Update the graphs
        foreach (var graph in graphs)
        {
            float val = graph.extractValueFunc(frame);
            graph.samples.samples.Add(new Sample() { millis = (int)frame.time, value = val });
            graph.instance.UpdateGraph();
        }
        lastSampleTime = (int)frame.time;
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

    void AddGraph(System.Func<AccelFrame, float> func, float minY, float maxY, Color color, string graphName)
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
