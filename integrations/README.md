# Emulator integration

It's possible to use the VGM player with an emulator and have the sound from real hardware while having the flexibility of an emulator.

## Instructions for DGen

- Download DGen from [here](https://sourceforge.net/projects/dgen/files/dgen/1.33/)
- Apply the patch `dgen-1.33.patch`
- Compile the application with `./configure --enable-vgmdump`
- Open the VGM Player and DGen in this order

## Instructions for RetroArch
- Follow [this](https://docs.libretro.com/development/retroarch/compilation/windows/) guide on how to build RetroArch
- Download the Genesis Plus GX core with `./libretro-fetch.sh genesis_plus_gx`
- Apply the patch `retroarch_genesis-plus-gx.patch`
- Compile the core and RetroArch
- Open the VGM Player and RetroArch in this order
