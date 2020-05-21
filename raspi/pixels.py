# Utility classes to communicate with pixels dices

from enum import IntEnum, unique

# We're using the bluepy lib for easy bluetooth access
# https://github.com/IanHarvey/bluepy
from bluepy.btle import Scanner, ScanEntry, Peripheral

# Pixels Bluetooth constants
PIXELS_SERVICE_UUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".lower()
PIXELS_SUBSCRIBE_CHARACTERISTIC = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".lower()
PIXELS_WRITE_CHARACTERISTIC = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E".lower()

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
    """Connection to a specific Pixel dice other Bluetooth"""

    def __init__(self, bluepy_entry: ScanEntry):
        assert bluepy_entry != None
        #for (adtype, desc, value) in bluepy_entry.getScanData():
        #    print(f"  {adtype} : {desc} => {value}")
        self._address = bluepy_entry.addr
        self._name = bluepy_entry.getValueText(8)
        self._device = Peripheral(bluepy_entry.addr, bluepy_entry.addrType)
        try:
            service = self._device.getServiceByUUID(PIXELS_SERVICE_UUID)
            if service:
                self._subscriber = service.getCharacteristics(PIXELS_SUBSCRIBE_CHARACTERISTIC)[0]
                self._writer = service.getCharacteristics(PIXELS_WRITE_CHARACTERISTIC)[0]
                self._send(MessageType.WhoAreYou)
                msg = list(self._subscriber.read())
                if msg[0] == MessageType.IAmADie:
                    self._dtype = DiceType(msg[1])
            if not hasattr(self, '_dtype'):
                raise Exception('Unexpected dice bluetooth characteristics')
        except:
            self._device.disconnect()
            self._device = None
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

    def play(self, index, remap_face = 0, loop = 0):
        self._send(MessageType.PlayAnim, index, remap_face, loop)

    def calibrate(self):
        self._send(MessageType.Calibrate)
        for i in range(2):
            msg = list(self._subscriber.read())
            while msg[0] != MessageType.NotifyUser:
                # Try again (why??)
                print(f"Unexpected message: {msg}")
                msg = list(self._subscriber.read())
            if not self._notify_user(msg):
                return False
        return True

    def _notify_user(self, msg):
        assert(msg[0] == MessageType.NotifyUser)
        #timeout, ok, cancel = msg[1:4]
        txt = bytes(msg[4:]).decode("utf-8")
        print(f'{txt} [Enter to continue, anything else to abort]: ')
        ok = PixelLink._get_continue()
        self._send(MessageType.NotifyUserAck, 1 if ok else 0)
        return ok

    def _send(self, message_type, *args):
        self._writer.write(bytes([message_type, *args]))

    @staticmethod
    def _get_continue():
        from getch import getch
        return getch() == '\r'


def enumerate_pixels(timeout_secs = 1):
    """Returns a list of Pixel dices discovered over Bluetooth"""
    devices = Scanner().scan(timeout_secs)
    pixels = []
    for dev in devices:
        #print(f"Device {dev.addr} ({dev.addrType}), RSSI={dev.rssi} dB")
        if dev.getValueText(7) == PIXELS_SERVICE_UUID:
            pixels.append(PixelLink(dev))
    return pixels


if __name__ == "__main__":
    print('Scanning for Pixels')
    pixels = enumerate_pixels()
    for d in pixels:
        print(f"Found Pixel dice: {d.address} => {d.name} of type {d.dtype.name}")
        #d.play(0)
        d.calibrate()
        break
