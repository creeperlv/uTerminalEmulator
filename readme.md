# uTerminalEmulator

uTerminalEmulator is an terminal emulator works in Unity3D!

## Demo

![Demo0](./docs/Example-000.gif)

## Usage

Recommended to use generated texture instead of texture in Assets for to be corrected rendered, the texture must be larger than Width*CW,Height*CH.

Use MaterialTextureApplier to apply the output texture to a material.

## Limitations

- Only support prerendered character map in char order, 0,0 is the top-left character.

- Terminal Size cannot be changed after Start() is called.

## VT100 or later support

This terminal emulator aims on partial VT100 function compatibility.

Implemented:

|Functions|Description|
| -- | --|
| ESC[0m| Reset FG and BG|
| ESC[3#m, ESC[9#m | Set FG|
| ESC[4#m, ESC[10#m | Set BG|
| ESC[#J| Only ESC[J is tested|
| ESC[A,B,C,D | Implemented for both input and output|
