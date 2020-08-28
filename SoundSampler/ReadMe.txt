Thank you for using my software! If you like my program, please consider a small donation to my PayPal: polishsirhawk@gmail.com.

This file contains basic control for the program, including handling Arduino code. You require .NET Framework 4.7.2 to be able to use my program
and FastLED library (https://github.com/FastLED/FastLED) to upload Arduino code to your microcontroler.

SoundSampler is an ultra light program that uses WASAPI to capture the sound directly from operating system, this bypasses the need for any 
extra sound source or mixing. Through CSCore library the sound data is processed with Fast Fourier Transform (FFT), sorted into 10 columns 
representing regular 10 band frequency bars. Afterwards, the highest column is being selected and sent through selected USB port. It does 
nothing else, which was the principle while making SoundSampler. Any change you make in the current session is saved and carried over to the next one
upon closing the application.

How to use:

Download the release version you're interested in, put the 3 files (CSCore.dll, CSCore.xml [not necessary], SoundSampler.exe) in one folder and double click on the executable.

SamplerApp (customizable version)
- When launched, the program will show up as an icon in the tray menu. If there is no USB device available connected, you will be greeted with an appropriate message;
- Press right mouse button to open context menu;
- In context menu you have to select COM port to send the data to, you can then select program calculation frequency, choose which columns to analyze (first 3 spectrum columns, so bass or 10 octaves) or to shut it dowm;
- Press left mouse button on the program tray icon to start/stop the program.

SamplerApp (Light version)
- Same as above, except it always analyze 10 octaves.

Before uploading the code to your Arduino:
- Change the NUM_LEDS number to a number of LEDs you are about to drive;
- LED_PIN value represents a digital pin in your Arduino/clone unit, which will send the signal to LED stripe controller. Bear in mind,
if you were to change that PIN it must be a PWM pin, which you can check that in your device pinout;
- Color is represented in RGB range, you have to input manually your desired color;
- The code uses FastLED library, which has a great variety of control, albeit in this use case only the brightness control is used.
