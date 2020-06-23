# Utility classes to communicate with pixels dices

# Standard lib
from enum import IntEnum, unique
import time
import asyncio
import threading
import traceback
import sys
import signal
from queue import Queue

# Our types
from utils import integer_to_bytes, Event
from color import Color32
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
    _None = 0
    _6 = 1
    _20 = 2


@unique
class MessageType(IntEnum):
    """Pixel dices Bluetooth messages identifiers"""
    _None = 0
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

    # TESTING
    SetAllLEDsToColor = 41
    AttractMode = 42
    PrintNormals = 43
    PrintA2DReadings = 44
    LightUpFace = 45

    Count = 46


class PixelLink:
    """
    Connection to a specific Pixel dice other Bluetooth
    This class is not thread safe (because bluepy.btle.Peripheral is not)
    """

    # Pixels Bluetooth constants
    PIXELS_SERVICE_UUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".lower()
    PIXELS_SUBSCRIBE_CHARACTERISTIC = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".lower()
    PIXELS_WRITE_CHARACTERISTIC = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E".lower()

    # We're limited to 20 bytes paquets size because Raspberry Pi Model 3B is using Bluetooth 4.1
    # so we're stuck making sure our BulkData paquet fits in the 20 byte, i.e. 16 bytes of payload
    PIXELS_MESSAGE_BULK_DATA_SIZE = 16

    # Default timeout in seconds used for waiting on a dice message
    DEFAULT_TIMEOUT = 3

    # Set to true to print messages content
    _trace = True

    _devices = []

    @staticmethod
    def _get_continue():
        from getch import getch
        return getch() == '\r'

    """ Use one of the Pixels.connect_xxx() methods to create a valid Pixel object """
    def __init__(self):
        self._address = None
        self._name = None
        self._device = None
        self._subscriber = None
        self._writer = None

        # Create the message map
        self._message_map = {}
        for i in range(MessageType.Count):
            self._message_map[i] = []

        # Setup events
        self.face_up_changed = Event()
        self.battery_voltage_changed = Event()

        self._dtype = None
        self._battery_voltage = -1
        self._face_up = 0

    @property
    def name(self) -> str:
        return self._name

    @property
    def address(self) -> str:
        return self._address

    @property
    def dtype(self) -> DiceType:
        return self._dtype

    @property
    def battery_voltage(self) -> float:
        """Battery voltage (usually between 2.5 and 4.2 volts)
        Associated event: battery_voltage_changed"""
        return self._battery_voltage

    @property
    def face_up(self) -> int:
        """Starts at 1, returns 0 if no face up
        Associated event: face_up_changed"""
        return self._face_up


    async def _wait_until(self, condition, timeout):
        """ Wait until the condition is true or the timeout expires"""
        t = time.perf_counter()
        start_time = t
        end_time = start_time + timeout
        while (not condition()) and (t < end_time):
            await asyncio.sleep(0)
            time.sleep(0.001)
            t = time.perf_counter()

        if not condition():
            # not sure if I should throw or just return the condition...
            raise Exception("Timeout while waiting for condition")

    def _send(self, message_type: MessageType, *args):
        if PixelLink._trace:
            print(f'{self.name} <= {message_type.name}: {", ".join([format(i, "02x") for i in args])}')
        Pixels.send_data(self, message_type, *args)

    async def _send_and_ack(self, msg_type: MessageType, msg_data, ack_type: MessageType, timeout = DEFAULT_TIMEOUT):
        assert(timeout >= 0)
        self._send(msg_type, *msg_data)
        ack_msg = None
        def on_message(self, msg):
            nonlocal ack_msg
            if not ack_msg and msg[0] == ack_type:
                ack_msg = msg
        self._message_map[ack_type].append(on_message)
        try:
            await self._wait_until(lambda: ack_msg != None, timeout)
        finally:
            self._message_map[ack_type].remove(on_message)
        return ack_msg

    def _process_message(self, msg):
        """Processes a message coming for the device and routes it to the proper message handler"""
        if PixelLink._trace:
            print(f'{self.name} => {MessageType(msg[0]).name}: {", ".join([format(i, "02x") for i in msg[1:]])}')

        handlers = self._message_map.get(msg[0])
        for handler in handlers:
            if handler != None:
                # Pass the message to the handler
                handler(self, msg)

    def _die_type_handler(self, msg):
        self._dtype = DiceType(msg[1])

    def _debug_log_handler(self, msg):
        endl = msg[1:].index(0) + 1 # find index of string terminator
        print(f'DEBUG[{self.address}]: {bytes(msg[1:endl]).decode("utf-8")}')

    def _battery_level_handler(self, msg):
        import struct
        voltage = struct.unpack('<f', bytes(msg[1:])) #little endian
        #print(f'Battery voltage: {voltage}')
        if self._battery_voltage != voltage:
            self._battery_voltage = voltage
            self.battery_voltage_changed.notify(voltage)

    def _state_handler(self, msg):
        state = msg[1]
        face = msg[2]
        #print(f'Face {face + 1} state {state}')
        face = face + 1 if state == 1 else 0
        if self._face_up != face:
            self._face_up = face
            self.face_up_changed.notify(face)

    def _notify_user_handler(self, msg):
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

    async def _upload_bulk_data(self, data: bytes, progress_callback, timeout = DEFAULT_TIMEOUT):
        assert(len(data))
        assert(timeout >= 0)
        # Send setup message
        await self._send_and_ack(MessageType.BulkSetup, integer_to_bytes(len(data), 2), MessageType.BulkSetupAck, timeout)
        # Then transfer data
        total_size = len(data)
        remainingSize = total_size
        offset = 0
        while remainingSize > 0:
            size = min(remainingSize, PixelLink.PIXELS_MESSAGE_BULK_DATA_SIZE)
            header = [size] + integer_to_bytes(offset, 2)
            await self._send_and_ack(MessageType.BulkData, header + data[offset:offset+size], MessageType.BulkDataAck, timeout)
            if progress_callback != None:
                progress_callback(offset, total_size)
            remainingSize -= size
            offset += size

    async def upload_animation_set(self, anim_set: AnimationSet, timeout = DEFAULT_TIMEOUT):
        data = []
        def append(dword):
            data.extend(integer_to_bytes(dword, 2))
        append(len(anim_set.palette))
        append(len(anim_set.keyframes))
        append(len(anim_set.rgb_tracks))
        append(len(anim_set.tracks))
        append(len(anim_set.animations))
        append(anim_set.heat_track_index)

        update_percent_increment = 0.1
        next_update_percent = update_percent_increment
        def print_progress(progress, total):
            nonlocal update_percent_increment
            nonlocal next_update_percent
            percent = progress / total
            if percent > next_update_percent:
                print(f"Uploading animation: {percent * 100:.2f}% complete")
                next_update_percent += update_percent_increment

        await self._send_and_ack(MessageType.TransferAnimSet, data, MessageType.TransferAnimSetAck, timeout)
        await self._upload_bulk_data(anim_set.pack(), print_progress, timeout)

    def await_upload_animation_set(self, anim_set: AnimationSet, timeout = DEFAULT_TIMEOUT):
        """ Kicks off a task to upload an animation set and waits for it to complete """
        return asyncio.run(self.upload_animation_set(anim_set, timeout))

    async def refresh_battery_voltage(self, timeout = DEFAULT_TIMEOUT):
        await self._send_and_ack(MessageType.RequestBatteryLevel, [], MessageType.BatteryLevel, timeout)
        return self.battery_voltage

    def await_refresh_battery_voltage(self, timeout = DEFAULT_TIMEOUT):
        """ Kicks off a task to refresh the battery voltage and waits for it to complete """
        return asyncio.run(self.refresh_battery_voltage(timeout))

    async def refresh_state(self, timeout = DEFAULT_TIMEOUT):
        await self._send_and_ack(MessageType.RequestState, [], MessageType.State, timeout)

    def await_refresh_state(self, timeout = DEFAULT_TIMEOUT):
        """ Kicks off a task to refresh the state and waits for it to complete """
        return asyncio.run(self.refresh_state(timeout))

    def request_telemetry(self, activate):
        self._send(MessageType.RequestTelemetry, 1 if activate else 0)

    def play(self, index, remap_face = 0, loop = 0):
        self._send(MessageType.PlayAnim, index, remap_face, loop)

    def stop(self, index, remap_face = 0):
        self._send(MessageType.StopAnim, index, remap_face)

    def play_event(self, event, remap_face = 0, loop = 0):
        self._send(MessageType.PlayAnimEvent, event, remap_face, loop)

    # Not working at the moment
    def force_LEDs_color(self, color: Color32):
        c = integer_to_bytes(color.to_rgb(), 4)
        self._send(MessageType.SetAllLEDsToColor, *c)

    def start_calibration(self):
        self._send(MessageType.Calibrate)

    def print_a2d_levels(self):
        self._send(MessageType.PrintA2DReadings)

    def light_up_face(self, face, color: Color32, remapFace = 255, layoutIndex = 255, remapRot = 255):
        data = []
        data.extend(integer_to_bytes(face, 1))
        data.extend(integer_to_bytes(remapFace, 1))
        data.extend(integer_to_bytes(layoutIndex, 1))
        data.extend(integer_to_bytes(remapRot, 1))
        data.extend(integer_to_bytes(color.to_rgb(), 4))
        self._send(MessageType.LightUpFace, *data)


class Pixels:
    """ Manages multiple pixels at once. Also supports the interactive mode """

    @unique
    class Command(IntEnum):
        _None = 0
        AddPixel = 1
        RemovePixel = 2
        SendMessage = 3
        TerminateThread = 4


    command_queue = Queue()

    connected_pixels = []
    available_pixels = []

    DEFAULT_SCAN_TIMEOUT = 3

    @staticmethod
    def enumerate_pixels(timeout = DEFAULT_SCAN_TIMEOUT):
        """Returns a list of Pixel dices discovered over Bluetooth"""
        print(f"Scanning BLE devices...")
        scanned_devices = Scanner().scan(timeout)
        Pixels.available_pixels.clear()
        for dev in scanned_devices:
            #print(f'Device {dev.addr} ({dev.addrType}), RSSI={dev.rssi} dB')
            if dev.getValueText(7) == PixelLink.PIXELS_SERVICE_UUID:
                name = dev.getValueText(8)
                Pixels.available_pixels.append(dev)
                print(f"Discovered Pixel {name}")
        return Pixels.available_pixels

    @staticmethod
    async def connect_by_name(pixel_name):
        """Connects to a single pixel, by name.
        This is a coroutine because connecting to the dice takes time."""
        for dev in Pixels.available_pixels:
            name = dev.getValueText(8)
            if name == pixel_name:
                return await Pixels.connect_pixel(dev)
        raise Exception(f"Could not find pixel named {pixel_name}")
        return None

    @staticmethod
    def await_connect_by_name(pixel_name):
        """Connects to a single pixel, by name."""
        return asyncio.run(Pixels.connect_by_name(pixel_name))

    @staticmethod
    async def connect_pixel(entry: ScanEntry) -> PixelLink:
        assert entry != None
        dice = PixelLink()

        finished = False
        def assigning_dev(ret_dev):
            nonlocal finished
            finished = True 

        # Get the BLE thread to connect to the device. We do this so the native code helper is attached to the BLE thread, not this one
        Pixels.command_queue.put([Pixels.Command.AddPixel, entry, dice, assigning_dev])
        while not finished:
            await asyncio.sleep(0)

        if dice._device != None:

            # register default message handlers
            dice._message_map[MessageType.IAmADie].append(PixelLink._die_type_handler)
            dice._message_map[MessageType.DebugLog].append(PixelLink._debug_log_handler)
            dice._message_map[MessageType.BatteryLevel].append(PixelLink._battery_level_handler)
            dice._message_map[MessageType.State].append(PixelLink._state_handler)
            dice._message_map[MessageType.NotifyUser].append(PixelLink._notify_user_handler)
            
            # Check type
            dice._dtype = None
            dice._send(MessageType.WhoAreYou)
            await dice._wait_until(lambda : dice._dtype != None, 10)
            if not dice._dtype:
                raise Exception("Pixel type couldn't be identified")

            # Battery level
            dice._battery_voltage = -1
            await dice.refresh_battery_voltage()

            # Face up (0 means no face up)
            dice._face_up = 0
            await dice.refresh_state()

            print(f"Dice {dice.name} connected")
            return dice
        else:
            return None


    @staticmethod
    def remove_pixel(pixel: PixelLink):
        Pixels.command_queue.put([Pixels.Command.RemovePixel, pixel])

    @staticmethod
    def send_data(pixel: PixelLink, message_type: MessageType, *args):
        Pixels.command_queue.put([Pixels.Command.SendMessage, pixel, message_type, *args])

    @staticmethod
    def _main():
        while True:
            # process queue of messages
            while Pixels.command_queue.qsize() > 0:
                cmd = Pixels.command_queue.get(False)
                if cmd[0] == Pixels.Command.AddPixel:
                    # extract parameters
                    bluepy_entry = cmd[1]
                    pixel = cmd[2]

                    # create the device
                    # pixel = PixelLink()
                    pixel._address = bluepy_entry.addr
                    pixel._name = bluepy_entry.getValueText(8)
                    pixel._device = Peripheral(bluepy_entry.addr, bluepy_entry.addrType)

                    print(f"Connecting to dice {pixel._name} at address {pixel._address}")
                    try:
                        # Get connected_pixels service
                        service = pixel._device.getServiceByUUID(PixelLink.PIXELS_SERVICE_UUID)
                        if not service:
                            raise Exception('Pixel service not found')

                        # Get the subscriber and writer for exchanging data with the dice
                        pixel._subscriber = service.getCharacteristics(PixelLink.PIXELS_SUBSCRIBE_CHARACTERISTIC)[0]
                        pixel._writer = service.getCharacteristics(PixelLink.PIXELS_WRITE_CHARACTERISTIC)[0]

                        # This magic code enables notifications from the subscribe characteristic,
                        # which in turn keeps the firmware on the dice from erroring out because
                        # it thinks it can't send notifications. Note that firmware code has also been
                        # fixed so it won't crash as a result :)
                        # There is an example at the bottom of the file of notifications working
                        pixel._device.writeCharacteristic(pixel._subscriber.valHandle + 1, b'\x01\x00')

                        # Bluepy notification delegate
                        class ProcessMessageDelegate(DefaultDelegate):
                            def handleNotification(self, cHandle, data):
                                pixel._process_message(list(data))
                        pixel._device.withDelegate(ProcessMessageDelegate())
                    except:
                        pixel._device.disconnect()
                        pixel._device = None
                        raise

                    # store the pixel
                    Pixels.connected_pixels.append(pixel)

                    # notify calling code
                    cmd[3](pixel._device)
                elif cmd[0] == Pixels.Command.RemovePixel:
                    pixel = cmd[1]
                    Pixels.connected_pixels.remove(pixel)
                    pixel._device.disconnect()
                elif cmd[0] == Pixels.Command.SendMessage:
                    pixel = cmd[1]
                    data = bytes(cmd[2:])
                    pixel._writer.write(data)
                elif cmd[0] == Pixels.Command.TerminateThread:
                    # close all connections
                    for pixel in Pixels.connected_pixels:
                        pixel._device.disconnect()
                    Pixels.connected_pixels.clear()
                    return

            # poll BLE stacks
            for pixel in Pixels.connected_pixels:
                pixel._device.waitForNotifications(0.0001) # 0 seems to cause issues

            # wait before looping again
            time.sleep(0.0001)

    @staticmethod
    def start():

        # install signal handler
        def signal_handler(signal, frame):
            Pixels.terminate()
            sys.exit(0)

        signal.signal(signal.SIGINT, signal_handler)

        # kick off thread
        t = threading.Thread(target=Pixels._main)
        t.start()

        # start a scan!
        Pixels.enumerate_pixels()

    @staticmethod
    def terminate():
        Pixels.command_queue.put([Pixels.Command.TerminateThread])

    @staticmethod
    def is_in_interpreter():
        interpreter = False
        try:
            interpreter = sys.ps1
        except AttributeError:
            interpreter = sys.flags.interactive
        return interpreter


pixels = []
color = Color32(255, 255, 0)
async def main():
    Pixels.start()  

    # pixels.append(await Pixels.connect_by_name("D_71"))
    # await pixels[0].refresh_battery_voltage()
    # dice2 = await PixelLink.connect_dice("D_55")
    # connected_pixels.append(dice2)
    # color = Color32(255, 255, 0)
    # dice2.light_up_face(0, color)

    #await dice1.refresh_battery_voltage()

    #await dice1.upload_animation_set(AnimationSet.from_json_file('D20_animation_set.json'))
    # await dice2.refresh_battery_voltage()

    # If we're in the interactive interpreter, don't terminate the BLE thread, use Ctrl-C instead
    # this way messages are still processed
    if not Pixels.is_in_interpreter():
        Pixels.terminate()

if __name__ == "__main__":
    asyncio.run(main()) 
