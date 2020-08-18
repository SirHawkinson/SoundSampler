Thank you for using my software!

This file contains basic control for the program, including handling Arduino code. You require .NET Framework 4.7.2 to be able to use my program
and FastLED library to be able to 

SoundSampler is an ultra light program that uses WASAPI to capture the sound directly from operating system, this bypasses the need for any 
extra sound source or mixing. Through CSCore library the sound data is processed with Fast Fourier Transform (FFT), sorted into 10 columns 
representing regular 10 band frequency bars. Afterwards, the highest column is being selected and sent through selected USB port. It does 
nothing else, which was the principle while making SoundSampler.

How to use:

SamplerApp
- Press right mouse button to open context menu;
- In context menu you can select COM port to send the data to, select program calculation frequency or to shut it dowm;
- Press left mouse button on the program tray icon to start the program.

Before uploading the code to your Arduino:
- Change the NUM_LEDS number to a number of LEDs you are about to drive;
- LED_PIN value represents a digital pin in your Arduino/clone unit, which will send the signal to LED stripe controller. Bear in mind,
if you were to change that PIN it must be a PWM pin, which you can check that in your device pinout;
- Color is represented in RGB range, you have to input manually your desired color;
- The code uses FastLED library, which has a great variety of control, albeit in this use case only the brightness control is used.