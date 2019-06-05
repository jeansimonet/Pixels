using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;



public class Telemetry : MonoBehaviour
{
    public TelemetryDie DiePrefab;
    public RectTransform GraphRoot;
    public Button scanButton;
    public Button recordButton;
    public Text scanText;
    public Text recordText;

    Central central;

    Dictionary<Die, TelemetryDie> discoveredDice;
    Dictionary<Die, TelemetryDie> trackedDice;

	void Awake()
	{
		central = GetComponent<Central>();
        discoveredDice = new Dictionary<Die, TelemetryDie>();
        trackedDice = new Dictionary<Die, TelemetryDie>();
    }

    class LowPass
    {
        public float param = 1.0f;
        public float value;

        public float Update(float val, float dt)
        {
            float p = param * dt;
            value = value * (1.0f - p) + val * p;
            return value;
        }
    }

    IEnumerator Start()
	{
        // Until we can properly record data, disable
        scanButton.enabled = false;
        recordButton.enabled = false;

        yield return new WaitUntil(() => central.state == CentralState.Idle);

        StartIdle();
    }

    void Update()
    {
    }

    void StartIdle()
    {
        scanText.text = "Scan";
        scanButton.enabled = true;
        scanButton.onClick.RemoveAllListeners();
        scanButton.onClick.AddListener(StartScanning);

        if (trackedDice.Count > 0)
        {
            recordText.text = "Record";
            recordButton.enabled = true;
            recordButton.onClick.RemoveAllListeners();
            recordButton.onClick.AddListener(StartRecording);
        }
    }

    void StartScanning()
    {
        central.BeginScanForDice(OnDieDiscovered);
        scanButton.enabled = true;
        scanText.text = "Stop";
        scanButton.onClick.RemoveAllListeners();
        scanButton.onClick.AddListener(StartConnecting);
    }

    void OnDieDiscovered(Die newDie)
    {
        // Only "discover" the die if we are not currently connected OR have it discovered
        if (!trackedDice.Concat(discoveredDice).Any(d => d.Key.address == newDie.address))
        {
            // Create ui for telemetry
            var dieUI = GameObject.Instantiate<TelemetryDie>(DiePrefab);
            dieUI.gameObject.name = newDie.name;
            dieUI.transform.SetParent(GraphRoot, false);
            dieUI.transform.localPosition = Vector3.zero;
            dieUI.transform.localRotation = Quaternion.identity;
            dieUI.transform.localScale = Vector3.one;

            discoveredDice.Add(newDie, dieUI);
        }
    }

    void StartConnecting()
    {
        StartCoroutine(ConnectToDice());
    }


    IEnumerator ConnectToDice()
    {
        scanButton.enabled = false;
        scanText.text = "Connecting";
        central.StopScanForDice();
        foreach (var d in discoveredDice)
        {
            bool connected = false;
            central.ConnectToDie(d.Key, (cdie) => connected = true, OnDieDisconnected);
            yield return new WaitUntil(() => connected);

            // Die is now tracked
            trackedDice.Add(d.Key, d.Value);

            // Register for telemetry events
            d.Key.OnTelemetry += OnDieTelemetryReceived;
        }
        discoveredDice.Clear();

        StartIdle();
    }

    void OnDieDisconnected(Die die)
    {
        if (trackedDice != null && trackedDice[die].gameObject != null)
        {
            GameObject.Destroy(trackedDice[die].gameObject);
            trackedDice.Remove(die);
        }
    }

    void OnDieTelemetryReceived(Die die, Vector3 acc, int millis)
    {
        trackedDice[die].OnTelemetryReceived(acc, millis);
    }

    public void StartRecording()
    {
        recordButton.enabled = true;
        recordButton.onClick.RemoveAllListeners();
        recordButton.onClick.AddListener(SaveToFile);
        recordText.text = "Save";
        scanButton.enabled = false;
        scanText.text = "Recording";
        foreach (var d in trackedDice)
        {
            d.Value.Clear();
        }

// #if UNITY_IOS
//         var vr = GetComponent<iVidCapPro>();
// 		vr.BeginRecordingSession ("DiceCapture",
// 			512, 512,
// 			30,
// 			iVidCapPro.CaptureAudio.Audio_Plus_Mic,
// 			iVidCapPro.CaptureFramerateLock.Unlocked);
// #endif
    }

    public void SaveToFile()
    {
        foreach (var die in trackedDice)
        {
            die.Value.SaveToFile(die.Key.name);
        }

// #if UNITY_IOS
// 		var vr = GetComponent<iVidCapPro>();
// 		int ignore;
// 		vr.EndRecordingSession (iVidCapPro.VideoDisposition.Save_Video_To_Album, out ignore);
// #endif
        StartIdle();
    }
}
