# Anomalous Adventure
This is a ray traced procedural JRPG style game. It shares a legacy and dependencies with Anomalous Medical, but is its own project with its own renderer baesd on Diligent Engine.

## Assets
You will also need the AdventureAssets repo to have the assets. Clone this to the same folder you clone this repository and the dependencies to. 

## Building
Building targets 64 bit win32 and is done with Visual Studio. You can run it on other platforms that support running win32 applications, like Linux with Proton.

First clone the Dependencies repo into the same folder as this one. You can build what is needed for just Adventure by running `AdventureOnly_x64.bat`.

After building the dependencies open the solution in Visual Studio and you can build it. Just be sure to set the x64 target.

## Hardware and Platform Info
 * This is tested on the Steam Deck, but has major issues in both Windows and Linux.
 * AMD Radeon 6000 series cards have small graphical artifacts in Windows, but otherwise work ok.
 * Nvidia cards work without issues in Windows.
 * Intel Arc cards work without issues in Windows.

This should work even on very small RT hardware like the steam deck or recent AMD APUs. While it is ray traced the workload is not actually very intense. The issues on the steam deck seem to be more drivers than actual performance potential. Some of the demos work a little better on that platform and run pretty well.

If you try a combo I don't have let me know how it goes and I can add it.

----------------------------------------------------------

This software was designed and built in sunny Florida, USA.