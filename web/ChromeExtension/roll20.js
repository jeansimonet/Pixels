'use strict';

if (typeof window.roll20PixelsLoaded == 'undefined') {
    var roll20PixelsLoaded = true;
    let log = console.log;

    function getArrayFirstElement(array) {
        //return (Array.isArray(array) && array.length) ? array[0] : undefined;
        return typeof array == "undefined" ? undefined : array[0];
    }

    function postChatMessage(message) {
        log("Posting message on Roll20: " + message);

        const chat = document.getElementById("textchat-input");
        const txt = getArrayFirstElement(chat?.getElementsByTagName("textarea"));
        const btn = getArrayFirstElement(chat?.getElementsByTagName("button"));
        //const speakingas = document.getElementById("speakingas");

        if ((typeof txt == "undefined") || (typeof btn == "undefined")) {
            log("Couldn't find Roll20 chat textarea and/or button");
        }
        else {
            const current_msg = txt.value;
            txt.value = message;
            btn.click();
            txt.value = current_msg;
        }
    }

    log("STARTING");

    var pixelServer = null;
    var formula = "@";
    let pixelStatus = 'Ready';

    const PIXELS_SERVICE_UUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()
    const PIXELS_SUBSCRIBE_CHARACTERISTIC = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()
    const PIXELS_WRITE_CHARACTERISTIC = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()

    async function listDevices() {
        let options = { filters: [{ services: [PIXELS_SERVICE_UUID] }] };
        log('Requesting Bluetooth Device with ' + JSON.stringify(options));

        disconnect();

        let service = await navigator.bluetooth.requestDevice(options)
            .then(device => {
                log('> Name:             ' + device.name);
                log('> Id:               ' + device.id);
                log('> Connected:        ' + device.gatt.connected);
                return device.gatt.connect();
            })
            .then(server => { pixelServer = server; return server.getPrimaryService(PIXELS_SERVICE_UUID); })
            .catch(error => log('Error connecting to Pixel: ' + error));

        if (service) {
            let _subscriber = await service.getCharacteristic(PIXELS_SUBSCRIBE_CHARACTERISTIC);
            let _writer = await service.getCharacteristic(PIXELS_WRITE_CHARACTERISTIC);

            updateStatus("Connected to Pixel");

            await _subscriber.startNotifications()
                .then(_ => {
                    log('Notifications started!');
                    _subscriber.addEventListener('characteristicvaluechanged', handleNotifications);
                })
                .catch(error => log('Error connecting to Pixel notifications: ' + error));
        }
    }

    function disconnect() {
        pixelServer?.disconnect();
        pixelServer = null;
        updateStatus('Not connected');
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

        log('Pixel notification: ' + arr.join(' '));

        if (value.getUint8(0) == 3) {
            handleFaceEvent(value.getUint8(1), value.getUint8(2))
        }
    }

    var hasMoved = false;

    function handleFaceEvent(ev, face) {
        if (!hasMoved) {
            if (ev != 1) {
                hasMoved = true;
            }
        }
        else if (ev == 1) {
            let txt = 'Face up: ' + (face + 1);
            log(txt);
            postChatMessage(formula.replace("@", face + 1));
            updateStatus(txt);
        }
    }

    function sendMessage() {
        // const buffer = new ArrayBuffer(16);
        // const int8View = new Int8Array(buffer);
        // int8View[0] = 1;
        // let r = await _writer.writeValue(buffer);
    }

    function updateStatus(status) {
        pixelStatus = status;
        log("New status: " + pixelStatus);
        sendMessageToExtension({ action: "showText", text: pixelStatus });
    }

    function sendMessageToExtension(data) {
        chrome.runtime.sendMessage(data);
    }

    updateStatus("Not connected");

    chrome.runtime.onMessage.addListener(function (msg, sender, sendResponse) {
        log("Received message from extension: " + msg.action);
        if (msg.action == "getStatus")
            sendMessageToExtension({ action: "showText", text: pixelStatus });
        if (msg.action == "setFormula")
            setFormula(msg.formula);
        else if (msg.action == "connect")
            connectToPixel();
        else if (msg.action == "disconnect")
            disconnectPixel();
    });

    function setFormula(f) {
        formula = f;
        log("Formula now is: " + formula);
    }

    function connectToPixel() {
        listDevices();
    }

    function disconnectPixel() {
        log("disconnect");
        disconnect();
    }

    // Disconnect
    // Formula
    // Multiple dice
}
