# ESP32/ESP8266 Exception Decoder for Generation Klick

ESPEDfGK will decode ESP8266 & ESP32 exceptions and show the source code where it happend.

It can be fully automated to a point where exceptions are catched automatically and passed to ESPEDfGK.

## the manual mode (don't do that!)

If an exception happens, copy the serial output from the IDE into the window of ESPEDfGK. Create an 
up to date binary (Alt + Ctrl + S). Decode.

## the serial mode (much better!)

Disconnect the Arduino IDE serial monitor, connect ESPEDfGKs serial monitor and wait for the exception to 
happen. Click the RED button on the serial monitor tab. Create an up to date binary (Alt + Ctrl + S). 
Decode.

## the Serial Monitor Mode (you will love it!)

Replace the Arduino IDE serial monitor with an new one: https://github.com/holgerlembke/serial-monitor 

It will catch exception automatically, no need to watch for it or wait for it. If exceptions happen they
will be tracked.

Click the RED button on the Decode tab. Create an up to date binary (Alt + Ctrl + S). Decode.

# Installation

On the Settings tab you need to add the path to __xtensa-esp-elf-addr2line.exe_ Just use the buttons, it 
might need some time to find all the variouse verions on you system. It does not matter much which you
choose, mosst of the times the freshes is a good choice.


This is a work in progress thing. I do enhancements, fixes and debugging as I need. 

# How it looks

![this is it](https://raw.githubusercontent.com/holgerlembke/ESPEDfGK/master/screenshots/ESPEDfGK1.jpg)

![this is it](https://raw.githubusercontent.com/holgerlembke/ESPEDfGK/master/screenshots/ESPEDfGK2.jpg)

# Install

* Download https://github.com/holgerlembke/ESPEDfGK/raw/master/release/files.zip
* Unzip at a convenient place.
* Use.

# Usage hint

* Ctrl+Scrollwheel to control UI sizing
* Close the Arduino IDE serial monitor!

# Dependancies

* at least .NET 6.0 is needed
* https://dotnet.microsoft.com/en-us/download
