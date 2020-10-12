'use strict';

function postChatMessage(message) {
    const chat = document.getElementById("textchat-input");
    const txt = chat.getElementsByTagName("textarea")[0];
    const btn = chat.getElementsByTagName("button")[0];
    const speakingas = document.getElementById("speakingas");

    const old_text = txt.value;
    txt.value = message;
    btn.click();
    txt.value = old_text;
}

let log = console.log;

const PIXELS_SERVICE_UUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()
const PIXELS_SUBSCRIBE_CHARACTERISTIC = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()
const PIXELS_WRITE_CHARACTERISTIC = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()

async function listDevices() {
    //let options = {acceptAllDevices: true};    
    let options = { filters: [{ services: [PIXELS_SERVICE_UUID] }] };
    log('Requesting Bluetooth Device with ' + JSON.stringify(options));

    let service = await navigator.bluetooth.requestDevice(options)
        .then(device => {
            log('> Name:             ' + device.name);
            log('> Id:               ' + device.id);
            log('> Connected:        ' + device.gatt.connected);
            return device.gatt.connect();
        })
        .then(server => server.getPrimaryService(PIXELS_SERVICE_UUID))
        .catch(error => log('Argh! ' + error));

    if (service) {
        let _subscriber = await service.getCharacteristic(PIXELS_SUBSCRIBE_CHARACTERISTIC);
        let _writer = await service.getCharacteristic(PIXELS_WRITE_CHARACTERISTIC);

        await _subscriber.startNotifications()
            .then(_ => {
                log('Notifications started!');
                _subscriber.addEventListener('characteristicvaluechanged', handleNotifications);
            });

        // const buffer = new ArrayBuffer(16);
        // const int8View = new Int8Array(buffer);
        // int8View[0] = 1;
        // let r = await _writer.writeValue(buffer);
    }
}

function handleNotifications(event) {
    let value = event.target.value;
    let arr = [];
    // Convert raw data bytes to hex values just for the sake of showing something.
    // In the "real" world, you'd use data.getUint8, data.getUint16 or even
    // TextDecoder to process raw data bytes.
    for (let i = 0; i < value.byteLength; i++) {
        arr.push('0x' + ('00' + value.getUint8(i).toString(16)).slice(-2));
    }
    log('Response: ' + arr.join(' '));
    if (value.getUint8(0) == 3) {
        handleFaceEvent(value.getUint8(1), value.getUint8(2))
    }
}

var hasMoved = false;

function handleFaceEvent(ev, face) {
    if (!hasMoved) {
        if (ev != 1) {
            log('moving');
            hasMoved = true;
        }
    }
    else if (ev == 1) {
        log('Face up');
        //var message = "!power {{--name|Pixel Roll --D20|[[ " + (face + 1) + "+ [[ @{tumble} ]] ]]}}"
        var message = "!power {{--name|Pixel Roll --Strength Check|[[ " + (face + 1) + "+ [[ 4 ]] ]]}}"
        postChatMessage(message);
    }
}

listDevices();
