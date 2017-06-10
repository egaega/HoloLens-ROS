# Set Up Development environment

## Check PC spec
***
### 1.CPU, GPU, BIOS spec
The HoloLens emulator is based on Hyper-V and uses RemoteFx for hardware accelerated graphics. 
#### To use the emulator, make sure your PC meets these hardware requirements:  
  * 64-bit Windows 10 Pro, Enterprise, or Education (The Home edition does not support Hyper-V or the HoloLens emulator)  
  * 64-bit CPU  
  * CPU with 4 cores (or multiple CPU's with a total of 4 cores)  
  * 8 GB of RAM or more  
#### In the BIOS, the following features must be supported and enabled:  
  * Hardware-assisted virtualization  
  * Second Level Address Translation (SLAT)  
  * Hardware-based Data Execution Prevention (DEP)  
#### GPU (The emulator might work with an unsupported GPU, but will be significantly slower)  
  * DirectX 11.0 or later  
  * WDDM 1.2 driver or later
  
If your system meets the above requirements, please ensure that the "Hyper-V" feature has been enabled on your system through Control Panel -> Programs -> Programs and Features -> Turn Windows Features on or off -> ensure that "Hyper-V" is selected for the Emulator installation to be successful.

Reference : https://developer.microsoft.com/en-us/windows/mixed-reality/install_the_tools
### 2.Activate "hyper-V"

## Install
***
### 1.Unity
### 2.Install VS2017 (or VS2015)
necessary package : Universal Windows Platform development, Game Development with Unity, Windows10 SDK, Windows8.1 SDK  
better? : deselect the Unity Editor optional component if installed a newer version of Unity

If you install vs2017, the following operations must be performed.  
 start "regedit"  
  HKEY_LOCAL_MACHINE > SOFTWARE > Microsoft > Analog  
  (if Analog don't exist, create Key  
  Add "string value"  
  name：OverrideHoloLensSDKPrerequisites  
  data：TRUE  
 
 do the same operation   
  HKEY_LOCAL_MACHINE > SOFTWARE > WOW6432Node > Microsoft > Analog 
  
  Reference : http://qiita.com/atsuo1203/items/6732110f6120ecbc2a76
### 3.Hololens Emulator
if hyper-V is not active, never end install.
### 4.Vuforia


## Outline
***
### 1.Unity :  
programing
### 2.vs2017 :  
unity's program export vs2017 and start emulator
