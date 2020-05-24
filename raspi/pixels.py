# Utility classes to communicate with pixels dices

from enum import IntEnum, unique

# We're using the bluepy lib for easy bluetooth access
# https://github.com/IanHarvey/bluepy
from bluepy.btle import Scanner, ScanEntry, Peripheral, DefaultDelegate


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
    """
    Connection to a specific Pixel dice other Bluetooth
    This class is not thread safe (because bluepy.btle.Peripheral is not)
    """

    def __init__(self, bluepy_entry: ScanEntry):
        assert bluepy_entry != None
        #for (adtype, desc, value) in bluepy_entry.getScanData():
        #    print(f"  {adtype} : {desc} => {value}")
        self._address = bluepy_entry.addr
        self._name = bluepy_entry.getValueText(8)
        self._device = Peripheral(bluepy_entry.addr, bluepy_entry.addrType)

        try:
            service = self._device.getServiceByUUID(PIXELS_SERVICE_UUID)
            if not service:
                raise Exception('Pixel service not found')

            self._subscriber = service.getCharacteristics(PIXELS_SUBSCRIBE_CHARACTERISTIC)[0]
            self._writer = service.getCharacteristics(PIXELS_WRITE_CHARACTERISTIC)[0]

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

    def wait_for_notifications(self, timeout):
        return self._device.waitForNotifications(timeout)

    def play(self, index, remap_face = 0, loop = 0):
        self._send(MessageType.PlayAnim, index, remap_face, loop)

    def calibrate(self):
        self._send(MessageType.Calibrate)

    def _send(self, message_type, *args):
        msg = [message_type, *args]
        self._writer.write(bytes(msg))

    def _process_message(self, msg):
        if msg[0] == MessageType.IAmADie:
            if not self._dtype:
                self._dtype = DiceType(msg[1])
        elif msg[0] == MessageType.DebugLog:
            print(f'DEBUG[{self.address}]: {bytes(msg[1:]).decode("utf-8")}')
        elif msg[0] == MessageType.State:
            self._update_state(*msg[1:])
        elif msg[0] == MessageType.NotifyUser:
            self._notify_user(msg)

    def _update_state(self, state, face):
        print(f'Face {face + 1} state {state}')

    def _notify_user(self, msg):
        assert(msg[0] == MessageType.NotifyUser)
        timeout, ok, cancel = msg[1:4]
        txt = bytes(msg[4:]).decode("utf-8")
        if ok and cancel:
            print(f'{txt} [Enter to continue, any other key to abort, timeout {timeout}s]:')
        else:
            print(f'{txt} [Enter to continue, timeout {timeout}s]:')
        ok = PixelLink._get_continue()
        self._send(MessageType.NotifyUserAck, 1 if ok else 0)
        return ok

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
    pixels = []
    while not pixels:
        pixels = enumerate_pixels()
        for dice in pixels:
            print(f"Found Pixel dice: {dice.address} => {dice.name} of type {dice.dtype.name}")
            break

        #pixels[0].calibrate()
        while True:
            if not pixels[0].wait_for_notifications(5):
                print("Waiting...")
