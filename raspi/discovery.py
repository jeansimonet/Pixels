# Install Remote Development Extension
# https://www.hanselman.com/blog/VisualStudioCodeRemoteDevelopmentOverSSHToARaspberryPiIsButter.aspx

# Select Python 3
# https://learn.sparkfun.com/tutorials/python-programming-tutorial-getting-started-with-the-raspberry-pi/configure-your-pi

# Getting started with bluepy => sudo pip3 install bluepy
# Be sure to run pip3 (not pip) and with sudo (if running with sudo later on)
# https://makersportal.com/blogsudo setcap 'cap_net_raw,cap_net_admin+eip' ${PY_SITE_PACKAGES_DIR}/2018/3/25/arduino-internet-of-things-part-4-connecting-bluetooth-nodes-to-the-raspberry-pi-using-pythons-bluepy-library
# /!\ run scripts with sudo

# Give user required privileges to access bluetooth
# https://github.com/IanHarvey/bluepy/issues/218
# https://stackoverflow.com/questions/59786226/how-to-setup-the-enviroment-that-bluepy-can-scan-without-sudo
# > sudo setcap 'cap_net_raw,cap_net_admin+eip' /home/pi/.local/lib/python3.7/site-packages/bluepy/bluepy-helper

# VS Code Python Language Server
# https://github.com/microsoft/vscode-python/issues/5969
# https://devblogs.microsoft.com/python/introducing-the-python-language-server/

from bluepy.btle import Scanner, DefaultDelegate, Peripheral
import time

class ScanDelegate(DefaultDelegate):
    def __init__(self):
        DefaultDelegate.__init__(self)

    def handleDiscovery(self, dev, isNewDev, isNewData):
        if isNewDev:
            print("Discovered device", dev.addr)
        elif isNewData:
            print("Received new data from", dev.addr)

scanner = Scanner().withDelegate(ScanDelegate())
devices = scanner.scan(1)

serviceGUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E"
subscribeCharacteristic = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E"
writeCharacteristic = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E"
MessageType_WhoAreYou = 1
MessageType_IAmADie = 2

def connectToDice(dev):
    print(f"Connecting to dice {dev.addr}")
    with Peripheral(dev.addr, dev.addrType) as periph:
        for serv in periph.getServices():
            if serv.uuid == serviceGUID.lower():
                subs = serv.getCharacteristics(subscribeCharacteristic)[0]
                write = serv.getCharacteristics(writeCharacteristic)[0]
                write.write(bytes([MessageType_WhoAreYou]))
                data = list(subs.read())
                if data[0] == MessageType_IAmADie:
                    print(f"I am a die of type {data[1]}")
                break

for dev in devices:
    print(f"Device {dev.addr} ({dev.addrType}), RSSI={dev.rssi} dB")
    for (adtype, desc, value) in dev.getScanData():
        #print(f"  {adtype} : {desc} => {value}")
        if value == serviceGUID.lower():
            connectToDice(dev)
