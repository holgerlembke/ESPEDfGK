# ESP32/ESP8266 Exception Decoder for Generation Klick

So in their endless wisdom the Arduino IDE 2.0 has different priorities than to recreate the 
old infrastructure and https://github.com/me-no-dev/EspExceptionDecoder does not work anymore.

And /me being too lazy to use the existing command line version from 
https://github.com/littleyoda/EspStackTraceDecoder does instead build a C# WPF app to help me.

Annoyingly the cut-and-paste from the serial monitor seems to have usage quirks I do not understand. So I added a serial monitor, too. It is very convinient, just click and go.

This is a work in progress thing. I do enhancements, fixes and debugging as I need. 

# How it looks

![this is it](https://raw.githubusercontent.com/holgerlembke/ESPEDfGK/master/screenshots/screen1.png)

![this is it](https://raw.githubusercontent.com/holgerlembke/ESPEDfGK/master/screenshots/screen2.png)

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
