# Bannerlord.BLSE
<p align="center">
  <a href="https://github.com/BUTR/Bannerlord.BLSE" alt="Lines Of Code">
    <img src="https://aschey.tech/tokei/github/BUTR/Bannerlord.BLSE?category=code" />
  </a>
  <a href="https://www.codefactor.io/repository/github/butr/bannerlord.blse">
    <img src="https://www.codefactor.io/repository/github/butr/bannerlord.blse/badge" alt="CodeFactor" />
  </a>
  <a href="https://codeclimate.com/github/BUTR/Bannerlord.BLSE/maintainability">
    <img alt="Code Climate maintainability" src="https://img.shields.io/codeclimate/maintainability-percentage/BUTR/Bannerlord.BLSE">
  </a>
  <a title="Crowdin" target="_blank" href="https://crowdin.com/project/blse">
    <img src="https://badges.crowdin.net/blse/localized.svg">
  </a>
  </br>
  <a href="https://github.com/BUTR/Bannerlord.BLSE/actions/workflows/test.yml?query=branch%3Amaster">
    <img alt="GitHub Workflow Status" src="https://img.shields.io/github/actions/workflow/status/BUTR/Bannerlord.BLSE/test.yml?branch=master&label=Game%20Stable%20and%20Beta">
  </a>
  </br>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/1" alt="NexusMods BLSE">
    <img src="https://img.shields.io/badge/NexusMods-BLSE-yellow.svg" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/1" alt="NexusMods BLSE">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-version-pzk4e0ejol6j.runkit.sh%3FgameId%3Dmountandblade2bannerlord%26modId%3D1" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/1" alt="NexusMods BLSE">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dunique%26gameId%3D3174%26modId%3D1" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/1" alt="NexusMods BLSE">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dtotal%26gameId%3D3174%26modId%3D1" />
  </a>
  <a href="https://www.nexusmods.com/mountandblade2bannerlord/mods/1" alt="NexusMods BLSE">
    <img src="https://img.shields.io/endpoint?url=https%3A%2F%2Fnexusmods-downloads-ayuqql60xfxb.runkit.sh%2F%3Ftype%3Dviews%26gameId%3D3174%26modId%3D1" />
  </a>
  </br>
  <!--
  <img src="https://staticdelivery.nexusmods.com/mods/3174/images/2513/2513-1612129311-35018174.png" width="800">
  -->
</p>

Extends the game.  

## Features
**BLSE** expands the game with the following features:

* **Interceptor** - BLSE checks if the is a class with a custom attribute named ***BLSEInterceptorAttribute***. If it's found it checks if there are the following signatures:
  *  **void OnInitializeSubModulesPrefix()** - will execute just before the game starts to initialize the SubModules. This gives us the ability to add SubModules declared in other programming languages like [Python](https://github.com/BUTR/Bannerlord.Python)﻿ and [Lua](https://github.com/BUTR/Bannerlord.Lua)﻿
  * **void OnLoadSubModulesPostfix()** - will execute just after all SubModules were initialized
* **LoadReferencesOnLoad** - gives the ability to add <Tag key="LoadReferencesOnLoad" value="false" /> that will disable explicit dependency load. Will be useful after switch to .NET Core runtime.

## Installation
Download the file and extract it's contents into the game's root folder (e.g. `C:\Program Files (x86)\Steam\steamapps\common\Mount & Blade II Bannerlord`).

## Troubleshooting
### Unblocking DLL's
You may need to right click on Bannerlord.BLSE.exe  file, click Properties, and click `Unblock` if you extracted the zip file with Windows Explorer or other programs that try to secure extracted files.
