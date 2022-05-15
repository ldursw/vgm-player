## VGM Player PCB

Schematic, KiCad 6 project and gerber files for the VGM player using Teensy 3.6.

This is a 2-layer board made with tolerances compatible with JLCPCB.

## Bill of materials

| Quantity | Item                                   | Notes                         |
|----------|----------------------------------------|-------------------------------|
| 55       | Femake header socket                   | 48 for teensy, 7 for SI5351   |
| 10       | Breakaway male headers                 | 4 for the SX1308 module       |
| 4        | 100k SMD 0805 resistor                 |                               |
| 3        | 10uF SMD 0805 ceramic capacitor        |                               |
| 3        | 0.1uF SMD 0805 ceramic capacitor       |                               |
| 2        | 47pF THT 2.54' pitch ceramic capacitor | Can be replaced with SMD 0805 |
| 2        | 1uF, 16V electrolytic capacitor        |                               |
| 2        | 10uF, 25V electrolytic capacitor       |                               |
| 2        | 2.2k SMD 0805 resistor                 |                               |
| 2        | 4.7k SMD 0805 resistor                 |                               |
| 2        | Jumper cap                             |                               |
| 1        | 2.1mm Barrel Jack                      |                               |
| 1        | SX1308 Step-Up Converter               |                               |
| 1        | 7805 Regulator, TO-220                 |                               |
| 1        | 100uH Inductor CD105                   |                               |
| 1        | Teensy 3.6 with pins                   |                               |
| 1        | TXS0108E, TSSOP-20                     |                               |
| 1        | 24-pin DIP socket, wide                |                               |
| 1        | 3.5mm headphone jack, female           |                               |
| 1        | AMS1117-3.3 regulator                  |                               |
| 1        | 330r SMD 0805 resistor                 |                               |
| 1        | 470r SMD 0805 resistor                 |                               |
| 1        | 680r SMD 0805 resistor                 |                               |
| 1        | TL072 op-amp                           |                               |
| 1        | Adafruit SI5351 Clock Generator        |                               |
| 1        | 74HC595 shift register, SOP-16         |                               |

## Assembly instructions

- Solder all components but leave the YM2612, Teensy, and clock generator disconnected.
- Make sure the jumper JP2 (near the 7805) is removed.
- Set the jumper J3 (near the barrel jack) to `* [* *] *` (EXT to center pin).
- Apply 5V to the barrel jack, center positive.
- Adjust the step-up module until it reaches 8V at the output.
- Remove power.
- Add a jumper to JP2 and move J3 to `[* *] * *` (VIN to center pin).
- Plug the Teensy, YM2612, and the clock generator into the board.
- Plug the headphone jack to line-in or a powered speaker.
- Connect the Teensy to the PC with an USB cable.
- Use the player in this repo to play music from file or emulator.

## 3D Render

![image](https://user-images.githubusercontent.com/37294448/168482891-bdb4865c-a57f-467d-841b-c584c2ed1121.png)

![image](https://user-images.githubusercontent.com/37294448/168482901-41df5e1e-9948-4844-89d6-3309abe0296c.png)
