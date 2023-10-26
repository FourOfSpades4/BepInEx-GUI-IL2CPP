# BepInEx GUI IL2CPP

Fork of [BepInEx.GUI](https://github.com/risk-of-thunder/BepInEx.GUI) by [xiaoxiao921](https://github.com/xiaoxiao921)

Made to work with any Unity game compiled using IL2CPP running [BepInEx 6.0 Bleeding Edge](https://builds.bepinex.dev/projects/bepinex_be) or newer. 

## Required Configuration

- Change [launch.rs](https://github.com/FourOfSpades4/BepInEx-GUI-IL2CPP/blob/main/bepinex_gui/src/config/launch.rs) to point to the correct **GAME_PATH** and **GAME_NAME** variables.
- Change [EntryPoint.cs](https://github.com/FourOfSpades4/BepInEx-GUI-IL2CPP/blob/main/BepInEx.GUI.Loader/src/EntryPoint.cs) to point to the correct **GAME_PATH** variable.
