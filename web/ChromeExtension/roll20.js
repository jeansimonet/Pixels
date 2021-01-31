'use strict';

if (typeof window.roll20PixelsLoaded == 'undefined') {
    var roll20PixelsLoaded = true;

    //
    // Helpers
    //

    let log = console.log;

    function getArrayFirstElement(array) {
        //return (Array.isArray(array) && array.length) ? array[0] : undefined;
        return typeof array == "undefined" ? undefined : array[0];
    }

    // Chat on Roll20
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

    //
    // Pixels bluetooth discovery
    //

    const PIXELS_SERVICE_UUID = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()
    const PIXELS_SUBSCRIBE_CHARACTERISTIC = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()
    const PIXELS_WRITE_CHARACTERISTIC = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E".toLowerCase()

    async function listDevices() {
        let options = { filters: [{ services: [PIXELS_SERVICE_UUID] }] };
        log('Requesting Bluetooth Device with ' + JSON.stringify(options));

        var pixelServer;
        var pixelName;
        let service = await navigator.bluetooth.requestDevice(options)
            .then(device => {
                log('> Name:             ' + device.name);
                log('> Id:               ' + device.id);
                log('> Connected:        ' + device.gatt.connected);
                pixelName = device.name;
                return device.gatt.connect();
            })
            .then(server => {
                pixelServer = server;
                return server.getPrimaryService(PIXELS_SERVICE_UUID);
            })
            .catch(error => log('Error connecting to Pixel: ' + error));

        if (service) {
            let _subscriber = await service.getCharacteristic(PIXELS_SUBSCRIBE_CHARACTERISTIC);
            //let _writer = await service.getCharacteristic(PIXELS_WRITE_CHARACTERISTIC);
            
            var pixel = new Pixel(pixelName, pixelServer);

            await _subscriber.startNotifications()
                .then(_ => {
                    log('Notifications started!');
                    _subscriber.addEventListener('characteristicvaluechanged', ev => pixel.handleNotifications(ev));
                })
                .catch(error => log('Error connecting to Pixel notifications: ' + error));

                sendTextToExtension('Just connected to ' + pixelName);
            pixels.push(pixel);
        }
    }

    //
    // Holds a bluetooth connection to a pixel dice
    //
    class Pixel {
        constructor(name, server) {
            this._name = name;
            this._server = server;
            this._hasMoved = false;
            this._status = 'Ready';
        }

        get isConnected() {
            return this._server != null;
        }

        get name() {
            return this._name;
        }

        get lastFaceUp() {
            return this._face;
        }

        disconnect() {
            this._server?.disconnect();
            this._server = null;
        }

        handleNotifications(event) {
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
                this._handleFaceEvent(value.getUint8(1), value.getUint8(2))
            }
        }
    
        _handleFaceEvent(ev, face) {
            if (!this._hasMoved) {
                if (ev != 1) {
                    this._hasMoved = true;
                }
            }
            else if (ev == 1) {
                this._face = face;
                let txt = this._name + ': face up = ' + (face + 1);
                log(txt);

                formula.replaceAll("#face_value", face + 1)
                    .replaceAll("#pixel_name", this._name)
                    .split("\\n")
                    .forEach(s => postChatMessage(s));

                sendTextToExtension(txt);
            }
        }
    
        // function sendMessage() {
        //     const buffer = new ArrayBuffer(16);
        //     const int8View = new Int8Array(buffer);
        //     int8View[0] = 1;
        //     let r = await _writer.writeValue(buffer);
        // }
    }

    //
    // Communicate with extension
    //

    function sendMessageToExtension(data) {
        chrome.runtime.sendMessage(data);
    }

    function sendTextToExtension(txt) {
        sendMessageToExtension({ action: "showText", text: txt });
    }

    function sendStatusToExtention() {
        sendTextToExtension(pixels.length + " pixels connected");
    }

    //
    // Initialize
    //

    log("Starting Pixels Roll20 extension");

    var pixels = []
    var formula = "#face_value";

    chrome.runtime.onMessage.addListener((msg, sender, sendResponse) => {
        log("Received message from extension: " + msg.action);
        if (msg.action == "getStatus") {
            sendStatusToExtention();            
        }
        else if (msg.action == "setFormula") {
            if (formula != msg.formula) {
                formula = msg.formula;
                log("Updated Roll20 formula: " + formula);
            }
        }
        else if (msg.action == "connect") {
            listDevices();
        }
        else if (msg.action == "disconnect") {
            log("disconnect");
            pixels.forEach(pixel => pixel.disconnect());
            pixels = []
            sendStatusToExtention();
        }
    });

    sendStatusToExtention();
}
