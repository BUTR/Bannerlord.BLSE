# Bannerlord.BLSE
<p align="center">
  <a href="https://github.com/BUTR/Bannerlord.UIExtenderEx" alt="Logo">
    <img src="https://media.discordapp.net/attachments/422092475163869201/1083742477250465843/BLSE_SMALL_SMALL.png" />
  </a>
  </br>
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

The Bannerlord Software Extender (BLSE) is a tool for Bannerlord mods that expands modding capabilities and adds additional functionality to the game.  
  
Once installed, no additional steps are needed to launch Bannerlord with BLSE's added functionality.  
You can start the game using **Bannerlord.BLSE.Launcher.exe** for the Vanilla Launcher or **Bannerlord.BLSE.LauncherEx.exe** for the Extended Launcher (BUTRLoader).  
Mod Developers can use **Bannerlord.BLSE.Standalone.exe** to use the CLI to launch the game.  
  
Credits to [Pickysaurus](https://www.nexusmods.com/users/31179975) for the BLSE and BUTR Logos!  


## Features
* **Unblocking Files** 
  * **Launcher** and **LauncherEx** will automatically unblock files on launch.   
Can be opted-out via passing **/nounblock** in command-line args.
  * Standalone will not automatically unblock files on launch.  
Can opted-in by passing **/unblock** in command-line args.
* **Continue Save File** - Allows to specify the save file to load when launching the game.  
  * Can be used by passing **/continuesave _mysavegame_** in command-line args.  
  * (**Standalone** Only) Passing the save file without the module list is also supported. The game will check all modules from the save file and load them automatically.  
* **DPI Aware** - Removes the blurry MessageBoxes and Crash Reports.
* **Game Pass PC** - Support of modding on the Xbox platform. BLSE disabled Xbox integration, replacing Cloud Saves with saves stored like on Steam/GOG/Epic  
* **Assembly Resolver** - Changes the game's assembly loading priority.  
  * If an assembly is available in one of the loaded modules, it will be loaded from there instead, even if the assembly is available in the main **/bin** folder.  
* **Interceptor** - BLSE checks if the is a class with a custom attribute named ***BLSEInterceptorAttribute***. If it's found it checks if there are the following signatures:  
  *  **static void OnInitializeSubModulesPrefix()** - will execute just before the game starts to initialize the SubModules. This gives us the ability to add SubModules declared in other programming languages like [Python](https://github.com/BUTR/Bannerlord.Python) and [Lua](https://github.com/BUTR/Bannerlord.Lua)  
  * **static void OnLoadSubModulesPostfix()** - will execute just after all SubModules were initialized  
  
## FAQ
* I have issues with the installation!
  * <details>
    <summary>Xbox Game Pass PC</summary>
      <p>You need to copy content of '/bin/Gaming.Desktop.x64_Shipping_Client' from BLSE to 'Mount & Blade II- Bannerlord/Content/bin/Gaming.Desktop.x64_Shipping_Client'</p>
      <img src="https://media.discordapp.net/attachments/422092475163869201/1088721252702765126/image.png" alt="BLSE Installation Path" width="500">
      <p>You need to copy content of 'Modules/Bannerlord.Harmony' from BLSE to 'Mount & Blade II- Bannerlord/Content/Modules/Bannerlord.Harmony'</p>
      <img src="https://media.discordapp.net/attachments/422092475163869201/1088721253692616775/image.png" alt="Bannerlord.Harmony Installation Path" width="500">
    </details>
  * <details>
    <summary>Steam</summary>
      <p>You need to copy content of '/bin/Win64_Shipping_Client' from BLSE to 'Mount & Blade II Bannerlord/bin/Win64_Shipping_Client'</p>
      <img src="https://media.discordapp.net/attachments/422092475163869201/1088721252962807818/image.png" alt="BLSE Installation Path" width="500">
      <p>You need to copy content of 'Modules/Bannerlord.Harmony' from BLSE to 'Mount & Blade II Bannerlord/Modules/Bannerlord.Harmony'</p>
      <img src="https://media.discordapp.net/attachments/422092475163869201/1088721253478711407/image.png" alt="Bannerlord.Harmony Installation Path" width="500">
    </details>
  * <details>
    <summary>GOG</summary>
      <p>You need to copy content of '/bin/Win64_Shipping_Client' from BLSE to 'Mount & Blade II Bannerlord/bin/Win64_Shipping_Client'</p>
      <img src="https://media.discordapp.net/attachments/422092475163869201/1088721253185097758/image.png" alt="BLSE Installation Path" width="500">
      <p>You need to copy content of 'Modules/Bannerlord.Harmony' from BLSE to 'Mount & Blade II Bannerlord/Modules/Bannerlord.Harmony'</p>
      <img src="https://media.discordapp.net/attachments/422092475163869201/1088725020458614794/image.png" alt="Bannerlord.Harmony Installation Path" width="500">
    </details>
* Do I need to include both `Win64_Shipping_Client` and `Gaming.Desktop.x64_Shipping_Client` directories?  
No!  
For Xbox Game Pass PC you need only `Gaming.Desktop.x64_Shipping_Client`  
For Steam/GOG/Epic you need only `Win64_Shipping_Client`  
* I don't see my old saves on Xbox Game Pass PC!  
BLSE uses a storage that Steam/GOG/Epic versions of the game use. We do not support Xbox's Cloud Saves!
* BLSE is not shown in Vortex's Tools!    
You need to add it manually for now!