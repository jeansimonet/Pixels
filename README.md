**Welcome to the Pixels dice repository!**

For more information on the dice, see Pixels' [website](https://www.pixels-dice.com/).

## Overview

This repository contains all the software developed for Pixels.

The software can be broken in two very distinct categories: code running on the Pixels (the firmware), and code running on computer (or mobile phone) that connects to the Pixels (the apps).

The communications are done over [Bluetooth Low Energy](https://en.wikipedia.org/wiki/Bluetooth_Low_Energy) (abbreviated to BLE). Pixels use a simple yet efficient messaging protocol that is easily portable to various languages such as C++, C#, python and Javascript.

Each top folder of the repository contains one or more different projects, here is the list:

 - blinky: a simple test written in C for Pixels' electronic board that blinks its LED #0
 - Bootloader: a C program that starts the main application uploaded in Pixels memory and allows updating that application over Bluetooth
 - Central: the mobile Pixels app, developed with Unity (so, written in C#)
 - DiceBLEWin: a C++ Unity plugin to expose the Windows BLE stack
 - Firmware: Pixel's firmware binaries and C++ sources
 - raspi: a python app that can connects to Pixels, runs on a Raspberry Pi 3 Model B+
 - web: currently a Chrome extension for Roll20 that connects to Pixels and outputs the dice rolls in the chat, written in Javascript

## Firmware
Pixels use a BLE module from Nordic. To build the firmware you'll have to install Nordic's nRF5 SDK. Download [here](https://www.nordicsemi.com/Software-and-tools/Software/Bluetooth-Software).

## Apps
### Central
You'll need Unity to build this project. It is currently using Unity 2020.1.

### Raspi
This project makes use of [bluepy](https://github.com/IanHarvey/bluepy) lib as its bluetooth stack. See pixels.py to get started.

### ChromeExtension
This Javascript extension is a proof of concept to showcase how Pixels can be used in conjunction with the Roll20 website.

## License
Unless noted overwise, those projects are licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.