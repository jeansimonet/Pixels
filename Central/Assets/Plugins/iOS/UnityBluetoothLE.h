//
//  UnityBluetoothLE.h
//  Unity-iPhone
//
//  Created by Tony Pitman on 03/05/2014.
//
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

@interface UnityBluetoothLE : NSObject <CBCentralManagerDelegate, CBPeripheralManagerDelegate, CBPeripheralDelegate>

{
    CBCentralManager *_centralManager;
    
    NSMutableDictionary *_peripherals;
    
#ifdef TARGET_OS_IOS
    CBPeripheralManager *_peripheralManager;
    
    NSString *_peripheralName;
    
    NSMutableDictionary *_services;
    NSMutableDictionary *_characteristics;
#endif
    
    NSMutableArray *_backgroundMessages;
    BOOL _isPaused;
    BOOL _alreadyNotified;
    BOOL _isInitializing;
    BOOL _rssiOnly;
    int  _recordType;
}

@property (atomic, strong) NSMutableDictionary *_peripherals;
@property (atomic) BOOL _rssiOnly;

- (void)initialize:(BOOL)asCentral asPeripheral:(BOOL)asPeripheral;
- (void)deInitialize;
- (void)scanForPeripheralsWithServices:(NSArray *)serviceUUIDs options:(NSDictionary *)options clearPeripheralList:(BOOL)clearPeripheralList recordType:(int)recordType;
- (void)stopScan;
- (void)retrieveListOfPeripheralsWithServices:(NSArray *)serviceUUIDs;
- (void)connectToPeripheral:(NSString *)name;
- (void)disconnectPeripheral:(NSString *)name;
- (CBCharacteristic *)getCharacteristic:(NSString *)name service:(NSString *)serviceString characteristic:(NSString *)characteristicString;
- (void)readCharacteristic:(NSString *)name service:(NSString *)serviceString characteristic:(NSString *)characteristicString;
- (void)writeCharacteristic:(NSString *)name service:(NSString *)serviceString characteristic:(NSString *)characteristicString data:(NSData *)data withResponse:(BOOL)withResponse;
- (void)subscribeCharacteristic:(NSString *)name service:(NSString *)serviceString characteristic:(NSString *)characteristicString;
- (void)unsubscribeCharacteristic:(NSString *)name service:(NSString *)serviceString characteristic:(NSString *)characteristicString;

#ifdef TARGET_OS_IOS
- (void)peripheralName:(NSString *)newName;
- (void)createService:(NSString *)uuid primary:(BOOL)primary;
- (void)removeService:(NSString *)uuid;
- (void)removeServices;
- (void)createCharacteristic:(NSString *)uuid properties:(CBCharacteristicProperties)properties permissions:(CBAttributePermissions)permissions value:(NSData *)value;
- (void)removeCharacteristic:(NSString *)uuid;
- (void)removeCharacteristics;
- (void)startAdvertising;
- (void)stopAdvertising;
- (void)updateCharacteristicValue:(NSString *)uuid value:(NSData *)value;
#endif

- (void)pauseMessages:(BOOL)isPaused;
- (void)sendUnityMessage:(BOOL)isString message:(NSString *)message;

+ (NSString *) base64StringFromData:(NSData *)data length:(int)length;

@end

@interface UnityMessage : NSObject

{
    BOOL _isString;
    NSString *_message;
}

- (void)initialize:(BOOL)isString message:(NSString *)message;
- (void)deInitialize;
- (void)sendUnityMessage;

@end
