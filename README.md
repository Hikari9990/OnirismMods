# OnirismMods
Compilation of mods for the game Onirism.

## Mods Description
### ColorSwap
Allows you to switch between the four different color palettes used for the co-op versions of Carol in singleplayer mode. Use F11 and F12 to cycle between them. Your current selection is saved in the mod's configuration file, so the change will persist between different sessions.
### FirstPerson
Adds a first person mode to the game, toggled by pressing X. Currently only works while aiming with a gun. Note that some weapons and outfits may get in the way of the camera, but it should generally work without too many issues.

#### These mods shouldn't affect saves in any way, but always remember to make a backup copy of your saves, just in case.

## Requirements
* [BepInEx 5.4.15][1]

## Installation
* Download BepInEx and extract the content of the archive in the main Onirism folder (where Onirism.exe is located)
* Download the desired plugin and extract the content of the archive in the respective subdirectories inside the BepInEx folder

## Configuration
Some of the aspects in each plugin are configurable, like key bindings and default values. To do that, simply edit the .cfg file located inside the "BepInEx\config" folder.
Downloading and installing [ConfigurationManager][2] will allow you to edit these settings directly from inside the game by pressing F1 to open the configuration manager. **WARNING: By default, the game doesn't work well with ConfigurationManager. Make sure your game is paused before opening the menu, or your cursor will be locked in the center of the screen!**

## Compiling
I haven't included any of the required libraries from the game or BepInEx inside this repository. If you want to compile from source, you'll need to copy the following DLLs inside the Libs folder:
#### From the "Onirism\Onirism_Data\Managed" folder:
* Assembly-CSharp.dll
* UnityEngine.dll
#### From the "BepInEx\Core" folder:
* 0Harmony.dll
* BepInEx.dll
* BepInEx.Harmony.dll


[1]: https://github.com/BepInEx/BepInEx/releases/tag/v5.4.15
[2]: https://github.com/BepInEx/BepInEx.ConfigurationManager