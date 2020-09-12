using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppConstants : SingletonMonoBehaviour<AppConstants>
{
    public TextAsset defaultDiceJson;
    public string DataSetFilename = "dice_data.json";
    public string SettingsFilename = "settings.json";
    public float ScanTimeout = 3.0f;
    public float DiceRotationSpeedAvg = 10.0f;
    public float DiceRotationSpeedVar = 1.0f;
    public Color DieUnavailableColor = Color.gray;
    public float DicePoolViewFirstScanDelay = 0.5f;
    public float DicePoolViewScanDelay = 30.0f;
    public float MultiDiceRootRotationSpeedAvg = 2.0f;
    public float MultiDiceRootRotationSpeedVar = 0.4f;
    public float DicePoolDisconnectDelay = 5.0f;
    public string AudioClipsFolderName = "AudioClips";
    public Color AudioClipsWaveformColor = Color.blue;
}

