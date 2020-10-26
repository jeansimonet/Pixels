using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace Dice
{
public partial class Die
	: MonoBehaviour
{
    public Coroutine UploadBulkData(byte[] bytes, System.Action<float> uploadPctCallback, System.Action<bool> resultCallback)
    {
        return PerformBluetoothOperation(UploadBulkDataCr(bytes, uploadPctCallback, resultCallback));
    }

    IEnumerator UploadBulkDataCr(byte[] bytes, System.Action<float> uploadPctCallback, System.Action<bool> resultCallback)
    {
        short remainingSize = (short)bytes.Length;

        Debug.Log("Sending " + remainingSize + " bulk data");
        // Send setup message
        var setup = new DieMessageBulkSetup();
        setup.size = remainingSize;
        bool acknowledged = false;
        yield return StartCoroutine(SendMessageWithAckRetryCr(
            setup,
            DieMessageType.BulkSetupAck,
            3,
            (ignore) =>
            {
                acknowledged = true;
            },
            null,
            null));

        if (acknowledged)
        {
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

                //Debug.Log("Sending Bulk Data (offset: 0x" + data.offset.ToString("X") + ", length: " + data.size + ")");
                //StringBuilder hexdumpBuilder = new StringBuilder();
                //for (int i = 0; i < data.data.Length; ++i)
                //{
                //    if (i % 8 == 0)
                //    {
                //        hexdumpBuilder.AppendLine();
                //    }
                //    hexdumpBuilder.Append(data.data[i].ToString("X02") + " ");
                //}
                //Debug.Log(hexdumpBuilder.ToString());

                acknowledged = false;
                yield return StartCoroutine(SendMessageWithAckRetryCr(
                    data,
                    DieMessageType.BulkDataAck,
                    3,
                    (ignore) =>
                    {
                        acknowledged = true;
                    },
                    null,
                    null));

                if (acknowledged)
                {
                    remainingSize -= data.size;
                    offset += data.size;
                    if (uploadPctCallback != null)
                    {
                        uploadPctCallback((float)offset/bytes.Length);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (acknowledged)
        {
            Debug.Log("Finished sending bulk data");
        }
        else
        {
            Debug.LogWarning("Error Uploading data");
        }
        resultCallback?.Invoke(acknowledged);
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

    public Coroutine UploadDataSet(DataSet set, System.Action<float> uploadPctCallback, System.Action<bool> callBack)
    {
        return PerformBluetoothOperation(UploadDataSetCr(set, uploadPctCallback, callBack));
    }

    IEnumerator UploadDataSetCr(DataSet set, System.Action<float> uploadPctCallback, System.Action<bool> callBack)
    {
        bool result = false;
        try
        {
            // Prepare the die
            var prepareDie = new DieMessageTransferAnimSet();
            prepareDie.paletteSize = set.animationBits.getPaletteSize();;
            prepareDie.rgbKeyFrameCount = set.animationBits.getRGBKeyframeCount();
            prepareDie.rgbTrackCount = set.animationBits.getRGBTrackCount();
            prepareDie.keyFrameCount = set.animationBits.getKeyframeCount();
            prepareDie.trackCount = set.animationBits.getTrackCount();
            prepareDie.animationCount = set.getAnimationCount();
            prepareDie.animationSize = (ushort)set.animations.Sum((anim) => Marshal.SizeOf(anim.GetType()));
            prepareDie.conditionCount = set.getConditionCount();
            prepareDie.conditionSize = (ushort)set.conditions.Sum((cond) => Marshal.SizeOf(cond.GetType()));
            prepareDie.actionCount = set.getActionCount();
            prepareDie.actionSize = (ushort)set.actions.Sum((action) => Marshal.SizeOf(action.GetType()));
            prepareDie.ruleCount = set.getRuleCount();
            //StringBuilder builder = new StringBuilder();
            //builder.AppendLine("Animation Data to be sent:");
            //builder.AppendLine("palette: " + prepareDie.paletteSize * Marshal.SizeOf<byte>());
            //builder.AppendLine("rgb keyframes: " + prepareDie.rgbKeyFrameCount + " * " + Marshal.SizeOf<Animations.RGBKeyframe>());
            //builder.AppendLine("rgb tracks: " + prepareDie.rgbTrackCount + " * " + Marshal.SizeOf<Animations.RGBTrack>());
            //builder.AppendLine("keyframes: " + prepareDie.keyFrameCount + " * " + Marshal.SizeOf<Animations.Keyframe>());
            //builder.AppendLine("tracks: " + prepareDie.trackCount + " * " + Marshal.SizeOf<Animations.Track>());
            //builder.AppendLine("animations: " + prepareDie.animationCount + ", " + prepareDie.animationSize);
            //builder.AppendLine("conditions: " + prepareDie.conditionCount + ", " + prepareDie.conditionSize);
            //builder.AppendLine("actions: " + prepareDie.actionCount + ", " + prepareDie.actionSize);
            //builder.AppendLine("rules: " + prepareDie.ruleCount + " * " + Marshal.SizeOf<Behaviors.Rule>());
            //builder.AppendLine("behavior: " + Marshal.SizeOf<Behaviors.Behavior>());
            //Debug.Log(builder.ToString());
            //Debug.Log("Animation Data size: " + set.ComputeDataSetDataSize());

            bool? acceptTransfer = null;
            yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(
                prepareDie,
                DieMessageType.TransferAnimSetAck,
                3.0f,
                (msg) => acceptTransfer = (msg as DieMessageTransferAnimSetAck).result == 0 ? false : true,
                null,
                null));
            if (acceptTransfer.HasValue)
            {
                if (acceptTransfer.Value)
                {
                    var setData = set.ToByteArray();
                    //StringBuilder hexdumpBuilder = new StringBuilder();
                    //for (int i = 0; i < setData.Length; ++i)
                    //{
                    //    if (i % 8 == 0)
                    //    {
                    //        hexdumpBuilder.AppendLine();
                    //    }
                    //    hexdumpBuilder.Append(setData[i].ToString("X02") + " ");
                    //}
                    //Debug.Log(hexdumpBuilder.ToString());

                    var hash = Utils.computeHash(setData);
                    Debug.Log("Die is ready to receive dataset, byte array should be: " + set.ComputeDataSetDataSize() + " bytes and hash 0x" + hash.ToString("X8"));

                    bool programmingFinished = false;
                    MessageReceivedDelegate programmingFinishedCallback = (finishedMsg) =>
                    {
                        programmingFinished = true;
                    };

                    AddMessageHandler(DieMessageType.TransferAnimSetFinished, programmingFinishedCallback);

                    yield return StartCoroutine(UploadBulkDataCr(
                        setData,
                        uploadPctCallback,
                        (res) => result = res));

                    if (result)
                    {
                        // We're done sending data, wait for the die to say its finished programming it!
                        Debug.Log("Done sending data, waiting for die to finish programming!");
                        yield return new WaitUntil(() => programmingFinished);
                        RemoveMessageHandler(DieMessageType.TransferAnimSetFinished, programmingFinishedCallback);
                    }
                    else
                    {
                        RemoveMessageHandler(DieMessageType.TransferAnimSetFinished, programmingFinishedCallback);
                        Debug.Log("Error!");
                    }
                }
                else
                {
                    Debug.Log("Transfer refused");
                }
            }
            else
            {
                Debug.Log("TimedOut");
            }
        }
        finally
        {
            callBack?.Invoke(result);
        }
    }

    public Coroutine PlayTestAnimation(DataSet testAnimSet, System.Action<bool> callBack)
    {
        return PerformBluetoothOperation(PlayTestAnimationCr(testAnimSet, callBack));
    }

    IEnumerator PlayTestAnimationCr(DataSet testAnimSet, System.Action<bool> callBack)
    {
        bool result = false;
        try
        {
            PixelsApp.Instance.ShowProgrammingBox("Uploading animation to " + name);

            // Prepare the die
            var prepareDie = new DieMessageTransferTestAnimSet();
            prepareDie.paletteSize = testAnimSet.animationBits.getPaletteSize();;
            prepareDie.rgbKeyFrameCount = testAnimSet.animationBits.getRGBKeyframeCount();
            prepareDie.rgbTrackCount = testAnimSet.animationBits.getRGBTrackCount();
            prepareDie.keyFrameCount = testAnimSet.animationBits.getKeyframeCount();
            prepareDie.trackCount = testAnimSet.animationBits.getTrackCount();
            prepareDie.animationSize = (ushort)Marshal.SizeOf(testAnimSet.animations[0].GetType());

            var setData = testAnimSet.ToTestAnimationByteArray();
            var hash = Utils.computeHash(setData);

            prepareDie.hash = hash;
            // Debug.Log("Animation Data to be sent:");
            // Debug.Log("palette: " + prepareDie.paletteSize * Marshal.SizeOf<byte>());
            // Debug.Log("rgb keyframes: " + prepareDie.rgbKeyFrameCount + " * " + Marshal.SizeOf<Animations.RGBKeyframe>());
            // Debug.Log("rgb tracks: " + prepareDie.rgbTrackCount + " * " + Marshal.SizeOf<Animations.RGBTrack>());
            // Debug.Log("keyframes: " + prepareDie.keyFrameCount + " * " + Marshal.SizeOf<Animations.Keyframe>());
            // Debug.Log("tracks: " + prepareDie.trackCount + " * " + Marshal.SizeOf<Animations.Track>());
            TransferTestAnimSetAckType acknowledge = TransferTestAnimSetAckType.NoMemory;
            yield return StartCoroutine(SendMessageWithAckOrTimeoutCr(
                prepareDie,
                DieMessageType.TransferTestAnimSetAck,
                3.0f,
                (ack) => acknowledge = ((DieMessageTransferTestAnimSetAck)ack).ackType,
                null,
                null));

            switch (acknowledge)
            {
                case TransferTestAnimSetAckType.Download:
                    {
                        Debug.Log("Die is ready to receive test dataset, byte array should be: " + setData.Length + " bytes and hash 0x" + hash.ToString("X8"));

                        bool programmingFinished = false; 
                        MessageReceivedDelegate programmingFinishedCallback = (finishedMsg) =>
                        {
                            programmingFinished = true;
                        };

                        AddMessageHandler(DieMessageType.TransferTestAnimSetFinished, programmingFinishedCallback);

                        yield return StartCoroutine(UploadBulkDataCr(
                            setData,
                            (pct) => PixelsApp.Instance.UpdateProgrammingBox(pct),
                            (res) => result = res));

                        if (result)
                        {
                            // We're done sending data, wait for the die to say its finished programming it!
                            Debug.Log("Done sending data, waiting for die to finish programming!");
                            yield return new WaitUntil(() => programmingFinished);
                            RemoveMessageHandler(DieMessageType.TransferTestAnimSetFinished, programmingFinishedCallback);
                        }
                        else
                        {
                            RemoveMessageHandler(DieMessageType.TransferTestAnimSetFinished, programmingFinishedCallback);
                            Debug.Log("Error!");
                        }
                    }
                    break;
                case TransferTestAnimSetAckType.UpToDate:
                    {
                        result = true;
                    }
                    break;
                default:
                    break;
            }
        }
        finally
        {
            PixelsApp.Instance.HideProgrammingBox();
            callBack?.Invoke(result);
        }
    }

    //public Coroutine UploadSettings(DieSettings settings, System.Action<bool> resultCallback)
    //{
    //    return PerformBluetoothOperation(UploadSettingsCr(settings, resultCallback));
    //}

    //IEnumerator UploadSettingsCr(DieSettings settings, System.Action<bool> resultCallback)
    //{
    //    // Prepare the die
    //    var prepareDie = new DieMessageTransferSettings();
    //    bool acknowledge = false;
    //    yield return StartCoroutine(SendMessageWithAckRetryCr(prepareDie, DieMessageType.TransferSettingsAck, 3, (ignore) => acknowledge = true, null, null));
        
    //    if (acknowledge)
    //    {
    //        // Die is ready, perform bulk transfer of the settings
    //        byte[] settingsBytes = DieSettings.ToByteArray(settings);
    //        yield return StartCoroutine(UploadBulkDataCr(settingsBytes, null, resultCallback));
    //    }
    //    else
    //    {
    //        resultCallback?.Invoke(false);
    //    }
    //}

    // public Coroutine DownloadSettings(System.Action<DieSettings> settingsReadCallback)
    // {
    //     return PerformBluetoothOperation(DownloadSettingsCr(settingsReadCallback));
    // }

    // IEnumerator DownloadSettingsCr(System.Action<DieSettings> settingsReadCallback)
    // {
    //     // Request the settings from the die
    //     yield return StartCoroutine(SendMessageWithAckRetryCr(new DieMessageRequestSettings(), DieMessageType.TransferSettings, null));

    //     // Got the message, acknowledge it
    //     PostMessage(new DieMessageTransferSettingsAck());

    //     byte[] settingsBytes = null;
    //     yield return StartCoroutine(DownloadBulkDataCr((buf) => settingsBytes = buf));
    //     var newSettings = DieSettings.FromByteArray(settingsBytes);

    //     // We've read the settings
    //     settingsReadCallback.Invoke(newSettings);
    // }

}
}