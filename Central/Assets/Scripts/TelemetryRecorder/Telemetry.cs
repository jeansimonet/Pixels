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

    Dictionary<Die, TelemetryDie> discoveredDice;
    Dictionary<Die, TelemetryDie> trackedDice;

	void Awake()
	{
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

        yield return new WaitUntil(() => Central.Instance.state == Central.State.Idle);

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
        DicePool.Instance.onDieCreated += OnDieDiscovered;
        DicePool.Instance.onDieAvailabilityChanged += OnDieAvailability;
        DicePool.Instance.BeginScanForDice();
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

    void OnDieAvailability(Die die, DicePool.DieAvailabilityState oldState, DicePool.DieAvailabilityState newState)
    {
        bool wasConnected = oldState == DicePool.DieAvailabilityState.Ready;
        bool isConnected = newState == DicePool.DieAvailabilityState.Ready;
        if (!wasConnected && isConnected)
        {
            // Die is now tracked
            trackedDice.Add(die, discoveredDice[die]);

            // Register for telemetry events
            die.OnTelemetry += OnDieTelemetryReceived;
        }
        else if (wasConnected && !isConnected)
        {
            if (trackedDice != null && trackedDice[die].gameObject != null)
            {
                GameObject.Destroy(trackedDice[die].gameObject);
                trackedDice.Remove(die);
            }
        }
    }

    void StartConnecting()
    {
        scanButton.enabled = false;
        scanText.text = "Connecting";
        DicePool.Instance.StopScanForDice();
        foreach (var d in discoveredDice)
        {
            DicePool.Instance.ConnectDie(d.Key);
        }
        StartIdle();
    }

    void OnDieTelemetryReceived(Die die, AccelFrame frame)
    {
        trackedDice[die].OnTelemetryReceived(frame);
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
