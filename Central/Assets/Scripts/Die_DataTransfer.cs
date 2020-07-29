using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;


public partial class Die
	: MonoBehaviour
    , Central.IDie
{
    public Coroutine UploadBulkData(byte[] bytes, System.Action<float> uploadPctCallback)
    {
        return PerformBluetoothOperation(UploadBulkDataCr(bytes, uploadPctCallback));
    }

    IEnumerator UploadBulkDataCr(byte[] bytes, System.Action<float> uploadPctCallback)
    {
        short remainingSize = (short)bytes.Length;

        Debug.Log("Sending " + remainingSize + " bulk data");
        // Send setup message
        var setup = new DieMessageBulkSetup();
        setup.size = remainingSize;
        yield return StartCoroutine(SendMessageWithAckRetryCr(setup, DieMessageType.BulkSetupAck, null));

        Debug.Log("Die is ready, sending data");

        // Then transfer data
        ushort offset = 0;
        while (remainingSize > 0)
        {
            var data = new DieMessageBulkData();
            data.offset = offset;
            data.size = (byte)Mathf.Min(remainingSize, DieMessages.maxDataSize);
            data.data = new byte[DieMessages.maxDataSize];
            System.Array.Copy(bytes, offset, data.data, 0, data.size);
            yield return StartCoroutine(SendMessageWithAckRetryCr(data, DieMessageType.BulkDataAck, null));
            remainingSize -= data.size;
            offset += data.size;
            if (uploadPctCallback != null)
            {
                uploadPctCallback((float)offset/bytes.Length);
            }
        }

        Debug.Log("Finished sending bulk data");
    }


    public Coroutine DownloadBulkData(System.Action<byte[]> onBufferReady)
    {
        return PerformBluetoothOperation(DownloadBulkDataCr(onBufferReady));
    }

    IEnumerator DownloadBulkDataCr(System.Action<byte[]> onBufferReady) 
    {
        // Wait for setup message
        short size = 0;
        yield return StartCoroutine(WaitForMessageCr(DieMessageType.BulkSetup, (msg) =>
        {
            var setupMsg = (DieMessageBulkSetup)msg;
            size = setupMsg.size;
        }));

        // Allocate a byte buffer
        byte[] buffer = new byte[size];
        ushort totalDataReceived = 0;

        // Setup bulk receive handler
        MessageReceivedDelegate bulkReceived = (msg) =>
        {
            var bulkMsg = (DieMessageBulkData)msg;
            System.Array.Copy(bulkMsg.data, 0, buffer, bulkMsg.offset, bulkMsg.size);

            // Create acknowledge message now
            var msgAck = new DieMessageBulkDataAck();
            msgAck.offset = totalDataReceived;

            // Sum data receive before sending any other message
            totalDataReceived += bulkMsg.size;

            // Send acknowledgment (no need to do it synchronously)
            PostMessage(msgAck);
        };
        AddMessageHandler(DieMessageType.BulkData, bulkReceived);

        // Send acknowledgement to the die, so it may transfer bulk data immediately
        PostMessage(new DieMessageBulkSetupAck());

        // Wait for all the bulk data to be received
        yield return new WaitUntil(() => totalDataReceived == size);

        // We're done
        RemoveMessageHandler(DieMessageType.BulkData, bulkReceived);
        onBufferReady.Invoke(buffer);
    }

    public Coroutine UploadAnimationSet(DataSet set, System.Action<float> uploadPctCallback, System.Action<bool> callBack)
    {
        return PerformBluetoothOperation(UploadAnimationSetCr(set, uploadPctCallback, callBack));
    }

    IEnumerator UploadAnimationSetCr(DataSet set, System.Action<float> uploadPctCallback, System.Action<bool> callBack)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferAnimSet();
        prepareDie.paletteSize = set.getPaletteSize();;
        prepareDie.keyFrameCount = set.getKeyframeCount();
        prepareDie.rgbTrackCount = set.getRGBTrackCount();
        prepareDie.animationCount = set.getAnimationCount();
        prepareDie.animationSize = (ushort)set.animations.Sum((anim) => Marshal.SizeOf(anim.GetType()));
        prepareDie.conditionCount = set.getConditionCount();
        prepareDie.conditionSize = (ushort)set.conditions.Sum((cond) => Marshal.SizeOf(cond.GetType()));
        prepareDie.actionCount = set.getActionCount();
        prepareDie.actionSize = (ushort)set.actions.Sum((action) => Marshal.SizeOf(action.GetType()));
        prepareDie.ruleCount = set.getRuleCount();
        prepareDie.behaviorCount = set.getBehaviorCount();
        prepareDie.currentBehaviorIndex = set.currentBehaviorIndex;
        prepareDie.heatTrackIndex = set.heatTrackIndex;
        Debug.Log("Animation Data to be sent:");
        Debug.Log("palette: " + prepareDie.paletteSize * Marshal.SizeOf<byte>());
        Debug.Log("keyframes: " + prepareDie.keyFrameCount + " * " + Marshal.SizeOf<Animations.RGBKeyframe>());
        Debug.Log("rgb tracks: " + prepareDie.rgbTrackCount + " * " + Marshal.SizeOf<Animations.RGBTrack>());
        Debug.Log("animations: " + prepareDie.animationCount + ", " + prepareDie.animationSize);
        Debug.Log("conditions: " + prepareDie.conditionCount + ", " + prepareDie.conditionSize);
        Debug.Log("actions: " + prepareDie.actionCount + ", " + prepareDie.actionSize);
        Debug.Log("rules: " + prepareDie.ruleCount + " * " + Marshal.SizeOf<Behaviors.Rule>());
        Debug.Log("behaviors: " + prepareDie.behaviorCount + " * " + Marshal.SizeOf<Behaviors.Behavior>());
        Debug.Log("current Behavior: " + prepareDie.currentBehaviorIndex);
        Debug.Log("heat track: " + prepareDie.heatTrackIndex);
        bool timeout = false;
        yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(prepareDie, DieMessageType.TransferAnimSetAck, 3.0f, null, () => timeout = true));
        if (!timeout)
        {
            Debug.Log("die is ready, sending data");
            Debug.Log("byte array should be: " + set.ComputeDataSetDataSize());
            var setData = set.ToByteArray();
            yield return StartCoroutine(UploadBulkDataCr(setData, uploadPctCallback));

            // We're done!
            Debug.Log("Done!");
            callBack?.Invoke(true);
        }
        else
        {
            Debug.Log("TimedOut");
            callBack?.Invoke(false);
        }
    }

    //public IEnumerator DownloadAnimationSet(AnimationSet outSet)
    //{
    //    // Request the anim set from the die
    //    SendMessage(new DieMessageRequestAnimSet());

    //    // Now wait for the setup message back
    //    int animCount = 0;
    //    yield return StartCoroutine(WaitForMessage(DieMessageType.TransferAnimSet, (msg) =>
    //    {
    //        var setupMsg = (DieMessageTransferAnimSet)msg;
    //        animCount = setupMsg.count;
    //    }));

    //    // Got the message, acknowledge it
    //    StartCoroutine(SendMessage(new DieMessageTransferAnimSetAck()));

    //    outSet.animations = new RGBAnimation[animCount];
    //    for (int i = 0; i < animCount; ++i)
    //    {
    //        byte[] animData = null;
    //        yield return StartCoroutine(DownloadBulkData((buf) => animData = buf));
    //        outSet.animations[i] = RGBAnimation.FromByteArray(animData);

    //        // Tell die we're ready for next anim
    //        StartCoroutine(SendMessage(new DieMessageTransferAnimReadyForNextAnim()));
    //    }

    //    // We've read all the anims!
    //}


    public Coroutine UploadSettings(DieSettings settings)
    {
        return PerformBluetoothOperation(UploadSettingsCr(settings));
    }

    IEnumerator UploadSettingsCr(DieSettings settings)
    {
        // Prepare the die
        var prepareDie = new DieMessageTransferSettings();
        yield return StartCoroutine(SendMessageWithAckRetryCr(prepareDie, DieMessageType.TransferSettingsAck, null));

        // Die is ready, perform bulk transfer of the settings
        byte[] settingsBytes = DieSettings.ToByteArray(settings);
        yield return StartCoroutine(UploadBulkDataCr(settingsBytes, null));

        // We're done!
    }

    public Coroutine DownloadSettings(System.Action<DieSettings> settingsReadCallback)
    {
        return PerformBluetoothOperation(DownloadSettingsCr(settingsReadCallback));
    }

    IEnumerator DownloadSettingsCr(System.Action<DieSettings> settingsReadCallback)
    {
        // Request the settings from the die
        yield return StartCoroutine(SendMessageWithAckRetryCr(new DieMessageRequestSettings(), DieMessageType.TransferSettings, null));

        // Got the message, acknowledge it
        PostMessage(new DieMessageTransferSettingsAck());

        byte[] settingsBytes = null;
        yield return StartCoroutine(DownloadBulkDataCr((buf) => settingsBytes = buf));
        var newSettings = DieSettings.FromByteArray(settingsBytes);

        // We've read the settings
        settingsReadCallback.Invoke(newSettings);
    }

}
