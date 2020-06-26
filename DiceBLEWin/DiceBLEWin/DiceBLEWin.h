#pragma once
#include <windows.h>
#include "IUnityInterface.h"

#include <vector>

typedef void(*DebugCallback)(const char* message);
typedef void(*SendBluetoothMessageCallback)(const char* objectName, const char* methodName, const char* message);

extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEConnectCallbacks(SendBluetoothMessageCallback sendMessageMethod, DebugCallback callbackMethod, DebugCallback warningMethod, DebugCallback errorMethod);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEDisconnectCallbacks();


void DebugLog(const char* message);
void DebugWarning(const char* message);
void DebugError(const char* message);
void SendBluetoothMessage(const char* message);

extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLELog(const char* message);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEInitialize(bool asCentral, bool asPeripheral);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEDeInitialize();
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEPauseMessages(bool isPaused);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEScanForPeripheralsWithServices(const char* serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLERetrieveListOfPeripheralsWithServices(const char* serviceUUIDsString);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEStopScan();
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEConnectToPeripheral(const char* name);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEDisconnectPeripheral(const char* name);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEReadCharacteristic(const char* name, const char* service, const char* characteristic);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEWriteCharacteristic(const char* name, const char* service, const char* characteristic, const unsigned char* data, int length, bool withResponse);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLESubscribeCharacteristic(const char* name, const char* service, const char* characteristic);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEUnSubscribeCharacteristic(const char* name, const char* service, const char* characteristic);
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEDisconnectAll();
extern "C" void UNITY_INTERFACE_EXPORT _winBluetoothLEUpdate();


extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces);
extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload();


