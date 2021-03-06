# VGM Player

This project allows VGM (Video Game Music) files from Sega Master System, Sega Game Gear,
and Sega Mega Drive/Genesis to be played on real hardware using the YM2612 sound chip and
the Teensy 3.6 board.

## Features

- Support for Master System, Game Gear, Mega Drive/Genesis
- Standalone player using the SD Card slot
- Can be controlled by a computer using the USB port
- Correct volume for PSG and FM
- Integration with emulators available
- Supports emulated and discrete SN76489
- Uses the Mega Amp circuit for amplification
- Hardware support for Teensy 3.6 and Arduino Due
- Player support for Windows and Linux (experimental)

## Supported boards

| Board | SD Card | Stream | Real PSG | Emulated PSG |
| --- | --- | --- | --- | --- |
| Teensy 3.6 | ✔️ | ✔️ | ✔️ | ✔️ |
| Arduino Due | ❔ | ✔️ | ✔️ | ✔️ |

✔️ Fully supported  
❔ Not tested  

## Demo
- [Instruction streaming from PC](https://www.youtube.com/watch?v=saAEF2lk2_Y)
- [Emulator with hardware sound](https://www.youtube.com/watch?v=Mupbj-XCC5c)
- [44.1kHz playback](https://www.youtube.com/watch?v=DkkqFxyVbDQ)
- [Music recordings](https://www.youtube.com/watch?v=EpjxUjE8uks)

## Difference between other implementations

### PSG

One of the biggest differences between this project and other implementations is that
an emulated PSG is used instead of the discrete SN76489. It is
[widely known](https://www.smspower.org/Development/SN76489#TheLinearFeedbackShiftRegister)
that the Sega version has some differences mainly in the noise channel.

For example, most games get a [weird artifact](https://gitcdn.link/cdn/ldursw/vgm-player/main/assets/panicpuppet-discrete.ogg) instead
of the [expected noise channel](https://gitcdn.link/cdn/ldursw/vgm-player/main/assets/panicpuppet-emulated.ogg).
After Burner II has a constant tone on a [discrete chip](https://gitcdn.link/cdn/ldursw/vgm-player/main/assets/afterburner-discrete.ogg)
whereas [on emulation](https://gitcdn.link/cdn/ldursw/vgm-player/main/assets/afterburner-emulated.ogg) it sounds right.

When using an emulated PSG the microcontroller will emulate the chip and the output is
sent to the `DAC0` pin at 44.1 kHz.

If you want to use a discrete PSG anyway, all you have to do is uncomment `-DUSE_REAL_PSG`
on `hardware/platformio.ini`, connect the `D0..D7` lines from the shift register to the
SN76489 and connect `/WE` to pin 29 though a voltage converter.

### Remote control

Another difference is that the hardware can be controlled via a companion application
written in .NET Core that accepts a generic input class and sends to the hardware via
USB and also displays the current state on screen.

![GUI](https://raw.githubusercontent.com/ldursw/vgm-player/main/assets/playergui.png)

There are 3 available input methods. `VgmFile` reads standard `.vgm` and `.vgz` files.
`VgmPipe` creates a named pipe `vgmstream` that accepts commands from other applications,
such as emulators. `VgmPCM` streams 44.1 kHz signed 8-bit mono PCM files to the chip.

## Compiling and Running the project

To compile the code for the hardware you will need [PlatformIO](https://platformio.org/).
You can use the CLI or an IDE with extension support such as Visual Studio Code. After
installing PlatformIO just open the `hardware` directory and upload the code to the board.

To compile the player application you will need the
[.NET Core SDK](https://dotnet.microsoft.com/download). You can use the CLI or an IDE
such as Visual Studio Community or Visual Studio Code. On Linux you may have to install
the `libSDL2` package.

## License

All files in this repository are licensed under GNU General Public License Version 3.

## Credits

Thank you very much for all these awesome people

- Maxim - PSG emulation code
- Dave, Maxim, Valley Bell - VGM Format
- zamaz - [DGen/SDL emulator](https://sourceforge.net/projects/dgen/files/dgen/1.33/)
- Softdev, Eke-Eke - [Genesis Plus GX emulator](https://github.com/ekeeke/Genesis-Plus-GX)
- Ace, Villahed94 - [Mega Amp](https://www.sega-16.com/forum/showthread.php?26568-Introducing-the-Mega-Amp-The-universal-Genesis-audio-circuit)
- Stephane-D - [XGM Tool and XGM Player](https://github.com/Stephane-D/SGDK/tree/master/sample/xgm-player)
- SDL Community - [SDL](https://www.libsdl.org/)
- greiman - [SdFat library](https://github.com/greiman/SdFat)
- ivanseidel - [DueTimer library](https://github.com/ivanseidel/DueTimer)
