using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class BluetoothLEHardwareInterface
{
	public enum CBCharacteristicProperties
	{
		CBCharacteristicPropertyBroadcast = 0x01,
		CBCharacteristicPropertyRead = 0x02,
		CBCharacteristicPropertyWriteWithoutResponse = 0x04,
		CBCharacteristicPropertyWrite = 0x08,
		CBCharacteristicPropertyNotify = 0x10,
		CBCharacteristicPropertyIndicate = 0x20,
		CBCharacteristicPropertyAuthenticatedSignedWrites = 0x40,
		CBCharacteristicPropertyExtendedProperties = 0x80,
		CBCharacteristicPropertyNotifyEncryptionRequired = 0x100,
		CBCharacteristicPropertyIndicateEncryptionRequired = 0x200,
	};

	public  enum CBAttributePermissions
	{
		CBAttributePermissionsReadable = 0x01,
		CBAttributePermissionsWriteable = 0x02,
		CBAttributePermissionsReadEncryptionRequired = 0x04,
		CBAttributePermissionsWriteEncryptionRequired = 0x08,
	};

	private static bool logAllMessages = false;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
	public delegate void DebugDelegate([MarshalAs(UnmanagedType.LPStr)]string message);
	public delegate void SendBluetoothMessageDelegate([MarshalAs(UnmanagedType.LPStr)]string gameObjectName, [MarshalAs(UnmanagedType.LPStr)]string methodName, [MarshalAs(UnmanagedType.LPStr)]string message);

	private static DebugDelegate _LogDelegate;
	private static DebugDelegate _WarningDelegate;
	private static DebugDelegate _ErrorDelegate;
	private static SendBluetoothMessageDelegate _SendMessageDelegate;

	private static readonly Queue<Action> _executionQueue = new Queue<Action>();




    private static void Enqueue(System.Action action)
	{
		lock(_executionQueue)
		{
			_executionQueue.Enqueue(action);
		}
	}

	private static void UnitySendMessageWrapper(string gameObjectName, string methodName, string message)
	{
		Enqueue(() =>
		{
			var gameObject = GameObject.Find(gameObjectName);
			if (gameObject != null)
			{
				gameObject.SendMessage(methodName, message);
			}
		});
	}

	private static void DebugLog(string message)
	{
		Debug.Log(message);
	}

	private static void DebugLogWarning(string message)
	{
		Debug.LogWarning(message);
	}

	private static void DebugLogError(string message)
	{
		Debug.LogError(message);
	}

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEConnectCallbacks(
		[MarshalAs(UnmanagedType.FunctionPtr)]SendBluetoothMessageDelegate sendMessage,
		[MarshalAs(UnmanagedType.FunctionPtr)]DebugDelegate log,
		[MarshalAs(UnmanagedType.FunctionPtr)]DebugDelegate warning,
		[MarshalAs(UnmanagedType.FunctionPtr)]DebugDelegate error);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLELog([MarshalAs(UnmanagedType.LPStr)]string message);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEInitialize(bool asCentral, bool asPeripheral);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEDeInitialize();

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEPauseMessages(bool isPaused);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEScanForPeripheralsWithServices([MarshalAs(UnmanagedType.LPStr)]string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLERetrieveListOfPeripheralsWithServices([MarshalAs(UnmanagedType.LPStr)]string serviceUUIDsString);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEStopScan();

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEConnectToPeripheral([MarshalAs(UnmanagedType.LPStr)]string name);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEDisconnectPeripheral([MarshalAs(UnmanagedType.LPStr)]string name);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEReadCharacteristic([MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string service, [MarshalAs(UnmanagedType.LPStr)]string characteristic);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEWriteCharacteristic([MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string service, [MarshalAs(UnmanagedType.LPStr)]string characteristic, byte[] data, int length, bool withResponse);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLESubscribeCharacteristic([MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string service, [MarshalAs(UnmanagedType.LPStr)]string characteristic);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEUnSubscribeCharacteristic([MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string service, [MarshalAs(UnmanagedType.LPStr)]string characteristic);

	[DllImport("DiceBLEWin")]
	private static extern void _winBluetoothLEDisconnectAll();

#elif ((UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX) && !UNITY_EDITOR_OSX
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLELog (string message);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEInitialize (bool asCentral, bool asPeripheral);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDeInitialize ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPauseMessages (bool isPaused);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopScan ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEConnectToPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectAll ();

#if !UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPeripheralName (string newName);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateService (string uuid, bool primary);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveService (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveServices ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateCharacteristic (string uuid, int properties, int permissions, byte[] data, int length);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristic (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristics ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStartAdvertising ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopAdvertising ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUpdateCharacteristicValue (string uuid, byte[] data, int length);
#endif
#elif UNITY_ANDROID
	static AndroidJavaObject _android = null;
#endif

	private static BluetoothDeviceScript bluetoothDeviceScript;

	public static void Log (string message)
	{
        if (logAllMessages)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            _winBluetoothLELog(message);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothLog", message);
#endif
        }
    }

	public static BluetoothDeviceScript Initialize (bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
	{
		bluetoothDeviceScript = null;

		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver == null)
		{
			bluetoothLEReceiver = new GameObject("BluetoothLEReceiver");

			bluetoothDeviceScript = bluetoothLEReceiver.AddComponent<BluetoothDeviceScript>();
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.InitializedAction = action;
				bluetoothDeviceScript.ErrorAction = errorAction;
			}
		}

		GameObject.DontDestroyOnLoad (bluetoothLEReceiver);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		// Register for updates
		_winBluetoothLEConnectCallbacks(UnitySendMessageWrapper, DebugLog, DebugLogWarning, DebugLogError);
		_winBluetoothLEInitialize(asCentral, asPeripheral);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEInitialize (asCentral, asPeripheral);
#elif UNITY_ANDROID
		if (_android == null)
		{
			AndroidJavaClass javaClass = new AndroidJavaClass ("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
			_android = javaClass.CallStatic<AndroidJavaObject> ("getInstance");
		}

		if (_android != null)
			_android.Call ("androidBluetoothInitialize", asCentral, asPeripheral);
#endif

		return bluetoothDeviceScript;
	}
	
	public static void DeInitialize (Action action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DeinitializedAction = action;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEDeInitialize();
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEDeInitialize ();
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothDeInitialize");
#endif
	}

	public static void FinishDeInitialize ()
	{
		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver != null)
			GameObject.Destroy(bluetoothLEReceiver);
	}

	public static void BluetoothEnable (bool enable)
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		//_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothEnable", enable);
#endif
}

	public static void PauseMessages (bool isPaused)
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEPauseMessages(isPaused);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEPauseMessages (isPaused);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothPause", isPaused);
#endif
	}

	public static void Update()
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				_executionQueue.Dequeue().Invoke();
			}
		}
#endif
	}

	public static void ScanForPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
	{
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.DiscoveredPeripheralAction = action;
			bluetoothDeviceScript.DiscoveredPeripheralWithAdvertisingInfoAction = actionAdvertisingInfo;

			if (bluetoothDeviceScript.DiscoveredDeviceList != null)
				bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
		}

		string serviceUUIDsString = null;

		if (serviceUUIDs != null && serviceUUIDs.Length > 0)
		{
			serviceUUIDsString = "";

			foreach (string serviceUUID in serviceUUIDs)
				serviceUUIDsString += serviceUUID + "|";

			serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEScanForPeripheralsWithServices(serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
#elif UNITY_ANDROID
		if (_android != null)
		{
			if (serviceUUIDsString == null)
				serviceUUIDsString = "";

			_android.Call ("androidBluetoothScanForPeripheralsWithServices", serviceUUIDsString, rssiOnly, recordType);
		}
#endif
	}

	public static void RetrieveListOfPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action)
	{
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.RetrievedConnectedPeripheralAction = action;
				
			if (bluetoothDeviceScript.DiscoveredDeviceList != null)
				bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
		}
			
		string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;
			
		foreach (string serviceUUID in serviceUUIDs)
			serviceUUIDsString += serviceUUID + "|";
			
		// strip the last delimeter
		serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLERetrieveListOfPeripheralsWithServices(serviceUUIDsString);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothRetrieveListOfPeripheralsWithServices", serviceUUIDsString);
#endif
	}

	public static void StopScan ()
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEStopScan();
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEStopScan ();
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothStopScan");
#endif
	}

	public static void DisconnectAll ()
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEDisconnectAll();
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEDisconnectAll ();
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothDisconnectAll");
#endif
	}

	public static void ConnectToPeripheral (string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
	{
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
			bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
			bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
			bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEConnectToPeripheral(name);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEConnectToPeripheral (name);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothConnectToPeripheral", name);
#endif
	}

	public static void DisconnectPeripheral (string name, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DisconnectedPeripheralAction = action;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEDisconnectPeripheral(name);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEDisconnectPeripheral (name);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidBluetoothDisconnectPeripheral", name);
#endif
	}

	public static void ReadCharacteristic (string name, string service, string characteristic, Action<string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
		{
			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][characteristic] = action;
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEReadCharacteristic(name, service, characteristic);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEReadCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidReadCharacteristic", name, service, characteristic);
#endif
	}

	public static void WriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DidWriteCharacteristicAction = action;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEWriteCharacteristic(name, service, characteristic, data, length, withResponse);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#endif
	}

	public static void SubscribeCharacteristic (string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
		{
			name = name.ToUpper ();
			service = service.ToUpper ();
			characteristic = characteristic.ToUpper ();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey(name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name][characteristic] = action;
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [characteristic] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [FullUUID (characteristic).ToLower ()] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLESubscribeCharacteristic(name, service, characteristic);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
	}

	public static void SubscribeCharacteristicWithDeviceAddress (string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
		{
			name = name.ToUpper ();
			service = service.ToUpper ();
			characteristic = characteristic.ToUpper ();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey(name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey(name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
			bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] = action;
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
			bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] = action;
#elif UNITY_ANDROID
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = notificationAction;
				
			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
			bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = action;
#endif
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLESubscribeCharacteristic(name, service, characteristic);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
	}

	public static void UnSubscribeCharacteristic (string name, string service, string characteristic, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
		{
			name = name.ToUpper ();
			service = service.ToUpper ();
			characteristic = characteristic.ToUpper ();

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey(name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey(name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;
#elif UNITY_ANDROID
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = null;
				
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID (characteristic).ToLower ()] = null;
#endif
		}

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		_winBluetoothLEUnSubscribeCharacteristic(name, service, characteristic);
#elif (UNITY_IPHONE || UNITY_TVOS) && !UNITY_EDITOR_OSX
		_iOSBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
		if (_android != null)
			_android.Call ("androidUnsubscribeCharacteristic", name, service, characteristic);
#endif
	}

	public static void PeripheralName (string newName)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLEPeripheralName (newName);
#endif
		}
	}

	public static void CreateService (string uuid, bool primary, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.ServiceAddedAction = action;

#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLECreateService (uuid, primary);
#endif
		}
	}
	
	public static void RemoveService (string uuid)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLERemoveService (uuid);
#endif
		}
	}

	public static void RemoveServices ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLERemoveServices ();
#endif
		}
	}

	public static void CreateCharacteristic (string uuid, CBCharacteristicProperties properties, CBAttributePermissions permissions, byte[] data, int length, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLECreateCharacteristic (uuid, (int)properties, (int)permissions, data, length);
#endif
		}
	}

	public static void RemoveCharacteristic (string uuid)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLERemoveCharacteristic (uuid);
#endif
		}
	}

	public static void RemoveCharacteristics ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLERemoveCharacteristics ();
#endif
		}
	}
	
	public static void StartAdvertising (Action action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.StartedAdvertisingAction = action;

#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLEStartAdvertising ();
#endif
		}
	}
	
	public static void StopAdvertising (Action action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.StoppedAdvertisingAction = action;

#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLEStopAdvertising ();
#endif
		}
	}
	
	public static void UpdateCharacteristicValue (string uuid, byte[] data, int length)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE && !UNITY_EDITOR_OSX
			_iOSBluetoothLEUpdateCharacteristicValue (uuid, data, length);
#endif
		}
	}
	
	public static string FullUUID (string uuid)
	{
		if (uuid.Length == 4)
			return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
		return uuid;
	}
}
