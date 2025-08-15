# CommandTools
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE) [![GitHub Tag](https://img.shields.io/github/v/tag/FireBlade211/CommandTools?label=Version)](https://github.com/FireBlade211/CommandTools/releases) [![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/FireBlade211/CommandTools/latest/total?label=Downloads%20(latest))](https://github.com/FireBlade211/CommandTools/releases)
<br>A suite of command-line (CLI) utilities for Windows.

# Table of Contents
- [(Top)](README.md#commandtools)
- [Installation](README.md#installation)
  - [Add to PATH](README.md#add-to-path)
- [License](README.md#license)
- [Contributions](README.md#contributions)
- [Future plans](README.md#future-plans)

# Installation
Go to the [Releases](https://github.com/FireBlade211/CommandTools/releases) page, find the latest release, download **`CommandToolsSetup.exe`**, run the file, and follow the steps inside Setup. Once installed, you can head over to the [Wiki](https://github.com/FireBlade211/CommandTools/wiki) for documentation, or use the `--help`, `-h`, or `-?` flag on any of the tools you installed. **Warning! Running the tools is only possible in the installation directory, unless you [Add to PATH](README.md#add-to-path).**

Example usage:<br>
```
run C:\Windows\System32\notepad.exe -t "Notepad with Custom Title!!!11!!!"
process list SELECT Name,PID
```

## Add to PATH
To allow running the CommandTools apps from any location, you need to add the CommandTools installation directory to your PATH environment variable by following the steps below. There are 3 ways to do it:
<details>
  <summary>
    <b>Run</b> way
  </summary>
  <ol>
    <li>Press <b>Ctrl + R</b> to open the <b>Run</b> dialog.</li>
    <li>Type <i>sysdm.cpl</i> and press <b>Enter</b>.</li>
    <li>Select the <b>Advanced</b> tab.</li>
    <li>Click <i>Environment Variables...</i></li>
    <li>Select <b>PATH</b> and press Edit. If you installed CommandTools for all users, select <b>PATH</b> under <i>System variables</i>. Otherwise, select the one under <i>User variables for [username].</i></li>
    <li>In the edit dialog that shows up, click <b>Browse</b> and select your CommandTools installation directory. Or, if you had your CommandTools installation directory copied, simply click <b>Add</b> and paste it in.</li>
    <li>Press <b>OK</b> to close the edit dialog, click it again to close the <b>Environment Variables</b> dialog, and press it one more time to close the <b>System Properties</b> dialog.</li>
  </ol>
</details>
<details>
  <summary>
    <b>Start Menu</b> way
  </summary>
  <ol>
    <li>Open the Start Menu, search for <b>System</b>, and open it.</li>
    <li>Select the <b>Advanced</b> tab.</li>
    <li>Click <i>Environment Variables...</i></li>
    <li>Select <b>PATH</b> and press Edit. If you installed CommandTools for all users, select <b>PATH</b> under <i>System variables</i>. Otherwise, select the one under <i>User variables for [username].</i></li>
    <li>In the edit dialog that shows up, click <b>Browse</b> and select your CommandTools installation directory. Or, if you had your CommandTools installation directory copied, simply click <b>Add</b> and paste it in.</li>
    <li>Press <b>OK</b> to close the edit dialog, click it again to close the <b>Environment Variables</b> dialog, and press it one more time to close the <b>System Properties</b> dialog.</li>
  </ol>
</details>
<details>
  <summary>
    <b>Registry</b> way (advanced, not recommended)
  </summary>
  <ol>
    <li>Press <b>Ctrl + R</b> to open the <b>Run</b> dialog, type in 'regedit', and press <b>Enter</b> to open <b>Registry Editor</b>.</li>
    <li>Inside the Registry Editor, go to different keys depending on your installation type. If you installed CommandTools for all users, go to <i>HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment</i>. If you installed only for your current user, go to <i>HKEY_CURRENT_USER\Environment</i>.</li>
    <li>Double-click on the <i>Path</i> (REG_SZ) value to edit it.</li>
    <li>In the edit dialog, add your CommandTools installation directory to the end of the string, followed by a semicolon (;).</li>
    <li>Press <b>OK</b> to close the edit dialog and save the changes, then close the Registry Editor.</li>
  </ol>
</details>

You may have to relaunch the Command Prompt for the changes to take effect, but once done, you should be able to launch the CommandTools apps from anywhere.

# License
**CommandTools** is open-source and available under the **MIT License**. See the [LICENSE](LICENSE) file for more details.

# Contributions
**CommandTools** is free and open-source. Contributions are welcome!

# Future plans
- Add SYSTEM and TrustedInstaller support to `run`
- Fix bugs and add more information in `sysinfo`
- Add more columns and subcommands to `process`
- Make and add more tools
