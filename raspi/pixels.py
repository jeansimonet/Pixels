# Utility classes to communicate with pixels dices

# Standard lib
from enum import IntEnum, unique
import time

# Our types
from utils import integer_to_bytes
from animation import AnimationSet

# We're using the bluepy lib for easy bluetooth access
# https://github.com/IanHarvey/bluepy
from bluepy.btle import Scanner, ScanEntry, Peripheral, DefaultDelegate


# Known issues:

# hci0 needs occasional reset 
#     devices = Scanner().scan(timeout_secs)
#   File "/home/pi/.local/lib/python3.7/site-packages/bluepy/btle.py", line 854, in scan
#     self.stop()
#   File "/home/pi/.local/lib/python3.7/site-packages/bluepy/btle.py", line 803, in stop
#     self._mgmtCmd(self._cmd()+"end")
#   File "/home/pi/.local/lib/python3.7/site-packages/bluepy/btle.py", line 312, in _mgmtCmd
#     raise BTLEManagementError("Failed to execute management command '%s'" % (cmd), rsp)
# bluepy.btle.BTLEManagementError: Failed to execute management command 'scanend' (code: 11, error: Rejected)
# https://github.com/zewelor/bt-mqtt-gateway/issues/59
# > sudo hciconfig hci0 reset


@unique
class DiceType(IntEnum):
    """Supported Pixel dices types"""
    _6 = 1
    _20 = 2


@unique
class MessageType(IntEnum):
    """Pixel dices Bluetooth messages identifiers"""
    WhoAreYou = 1
    IAmADie = 2
    State = 3
    Telemetry = 4
    BulkSetup = 5
    BulkSetupAck = 6
    BulkData = 7
    BulkDataAck = 8
    TransferAnimSet = 9
    TransferAnimSetAck = 10
    TransferSettings = 11
    TransferSettingsAck = 12
    DebugLog = 13
    PlayAnim = 14
    PlayAnimEvent = 15
    StopAnim = 16
    RequestState = 17
    RequestAnimSet = 18
    RequestSettings = 19
    RequestTelemetry = 20
    ProgramDefaultAnimSet = 21
    ProgramDefaultAnimSetFinished = 22
    Flash = 23
    FlashFinished = 24
    RequestDefaultAnimSetColor = 25
    DefaultAnimSetColor = 26
    RequestBatteryLevel = 27
    BatteryLevel = 28
    Calibrate = 29
    CalibrateFace = 30
    NotifyUser = 31
    NotifyUserAck = 32
    TestHardware = 33
    SetStandardState = 34
    SetLEDAnimState = 35
    SetBattleState = 36
    ProgramDefaultParameters = 37
    ProgramDefaultParametersFinished = 38


class PixelLink:
    """
    Connection to a specific Pixel dice other Bluetooth
    This class is not thread safe (because bluepy.btle.Peripheral is not)
    """

    # Pixels Bluetooth constants
    PIXELS_SERVICE_UUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".lower()
    PIXELS_SUBSCRIBE_CHARACTERISTIC = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".lower()
    PIXELS_WRITE_CHARACTERISTIC = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E".lower()

    # Not completely sure why, but it seems Blupy doesn't like to send BLE paquets greater than 20 bytes without a response
    # and the dice do not like to send responses, so we're stuck making sure our BulkData paquet fits in the 20 byte, i.e. 16 bytes of payload
    PIXELS_MESSAGE_BULK_DATA_SIZE = 16

    # Set to true to print messages content
    _trace = True

    @staticmethod
    def enumerate_pixels(timeout_secs = 1):
        """Returns a list of Pixel dices discovered over Bluetooth"""
        devices = Scanner().scan(timeout_secs)
        pixels = []
        for dev in devices:
            #print(f'Device {dev.addr} ({dev.addrType}), RSSI={dev.rssi} dB')
            if dev.getValueText(7) == PixelLink.PIXELS_SERVICE_UUID:
                pixels.append(PixelLink(dev))
        return pixels

    @staticmethod
    def _get_continue():
        from getch import getch
        return getch() == '\r'

    def __init__(self, bluepy_entry: ScanEntry):
        assert bluepy_entry != None
        #for (adtype, desc, value) in bluepy_entry.getScanData():
        #    print(f'> {adtype} : {desc} => {value}')
        self._address = bluepy_entry.addr
        self._name = bluepy_entry.getValueText(8)
        self._device = Peripheral(bluepy_entry.addr, bluepy_entry.addrType)

        try:
            service = self._device.getServiceByUUID(PixelLink.PIXELS_SERVICE_UUID)
            if not service:
                raise Exception('Pixel service not found')

            self._subscriber = service.getCharacteristics(PixelLink.PIXELS_SUBSCRIBE_CHARACTERISTIC)[0]
            self._writer = service.getCharacteristics(PixelLink.PIXELS_WRITE_CHARACTERISTIC)[0]

            # This magic code enables notifications from the subscribe characteristic,
            # which in turn keeps the firmware on the dice from erroring out because
            # it thinks it can't send notifications. Note that firmware code has also been
            # fixed so it won't crash as a result :)
            # There is an example at the bottom of the file of notifications working
            self._device.writeCharacteristic(self._subscriber.valHandle + 1, b'\x01\x00')

            # Bluepy notification delegate
            myPixel = self
            class PrintMessageDelegate(DefaultDelegate):
                def handleNotification(self, cHandle, data):
                    myPixel._process_message(list(data))
            self._device.withDelegate(PrintMessageDelegate())

            # Check type
            self._dtype = None
            self._send(MessageType.WhoAreYou)
            self.wait_for_notifications(1) # The 'IAmADie' message seems to always be the first one
            if not hasattr(self, '_dtype'):
                raise Exception("Pixel type couldn't be identified")

            # Battery level
            self._battery_voltage = -1
            self.refresh_battery_voltage()
            self.wait_for_notifications(1)

        except:
            self._device.disconnect()
            raise

    @property
    def name(self):
        return self._name

    @property
    def address(self):
        return self._address

    @property
    def dtype(self):
        return self._dtype

    @property
    def battery_voltage(self):
        return self._battery_voltage

    def wait_for_notifications(self, timeout):
        return self._device.waitForNotifications(timeout)

    def play(self, index, remap_face = 0, loop = 0):
        self._send(MessageType.PlayAnim, index, remap_face, loop)

    def calibrate(self):
        self._send(MessageType.Calibrate)

    def refresh_battery_voltage(self):
        self._send(MessageType.RequestBatteryLevel)

    def upload_animation_set(self, anim_set: AnimationSet):
        data = []
        def append(dword):
            data.extend(integer_to_bytes(dword, 2))
        append(len(anim_set.palette))
        append(len(anim_set.keyframes))
        append(len(anim_set.rgb_tracks))
        append(len(anim_set.tracks))
        append(len(anim_set.animations))
        append(anim_set.heat_track_index)
        self._send_and_ack(MessageType.TransferAnimSet, data, MessageType.TransferAnimSetAck, 3)
        self._upload_bulk_data(anim_set.pack())

    def _send(self, message_type: MessageType, *args):
        if PixelLink._trace:
            print(f'<= {message_type.name}: {", ".join([format(i, "02x") for i in args])}')
        data = bytes([message_type, *args])
        # assert(len(data) < ???)
        self._writer.write(data)

    def _send_and_ack(self, msg_type: MessageType, msg_data, ack_type: MessageType, timeout):
        intial_timeout = timeout
        self._send(msg_type, *msg_data)
        self._last_msg = None
        last_time = time.perf_counter()
        while timeout >= 0:
            if not self.wait_for_notifications(timeout):
                break
            if self._last_msg[0] == ack_type:
                return self._last_msg
            t = time.perf_counter()
            timeout -= t - last_time
            last_time = t
        raise Exception(f'Acknowledgement message of type {ack_type.name} not received before timeout of {intial_timeout}s')

    def _process_message(self, msg):
        if PixelLink._trace:
            print(f'=> {MessageType(msg[0]).name}: {", ".join([format(i, "02x") for i in msg[1:]])}')
        if msg[0] == MessageType.IAmADie:
            if not self._dtype:
                self._dtype = DiceType(msg[1])
        elif msg[0] == MessageType.DebugLog:
            print(f'DEBUG[{self.address}]: {bytes(msg[1:]).decode("utf-8")}')
        elif msg[0] == MessageType.BatteryLevel:
            import struct
            self._update_battery_voltage(*struct.unpack('<f', bytes(msg[1:]))) #little endian
        elif msg[0] == MessageType.State:
            self._update_state(*msg[1:])
        elif msg[0] == MessageType.NotifyUser:
            self._notify_user(msg)
        else:
            #TODO event
            self._last_msg = msg

    def _update_state(self, state, face):
        print(f'Face {face + 1} state {state}')

    def _update_battery_voltage(self, voltage):
        self._battery_voltage = voltage
        print(f'Battery voltage: {voltage}')

    def _notify_user(self, msg):
        assert(msg[0] == MessageType.NotifyUser)
        timeout, ok, cancel = msg[1:4]
        txt = bytes(msg[4:]).decode("utf-8")
        can_abort = ok and cancel
        txt_key = 'Enter to continue, any other key to abort' if can_abort else 'Any key to continue'
        print(f'{txt} [{txt_key}, timeout {timeout}s]:')
        ok = PixelLink._get_continue()
        if not can_abort:
            ok = True
        print("Continuing" if ok else "Aborting")
        self._send(MessageType.NotifyUserAck, 1 if ok else 0)
        return ok

    def _upload_bulk_data(self, data: bytes):
        assert(len(data))
        # Send setup message
        self._send_and_ack(MessageType.BulkSetup, integer_to_bytes(len(data), 2), MessageType.BulkSetupAck, 3)
        # Then transfer data
        remainingSize = len(data)
        offset = 0
        while remainingSize > 0:
            size = min(remainingSize, PixelLink.PIXELS_MESSAGE_BULK_DATA_SIZE)
            header = [size] + integer_to_bytes(offset, 2)
            self._send_and_ack(MessageType.BulkData, header + data[offset:offset+size], MessageType.BulkDataAck, 10)
            remainingSize -= size
            offset += size


if __name__ == "__main__":
    pixels = []
    while not pixels:
        print('Scanning for Pixels...')
        pixels = PixelLink.enumerate_pixels()
        for dice in pixels:
            print(f'Found Pixel dice: {dice.address} => {dice.name} of type {dice.dtype.name}')
            break

    #pixels[0].calibrate()
    pixels[0].upload_animation_set(AnimationSet.from_json_file('D20_animation_set.json'))
    # while True:
    #     if not pixels[0].wait_for_notifications(5):
    #         pixels[0].refresh_battery_voltage()
    #         print('Waiting...')
