![WaveEngine.MRTK](Documentation/Images/Wave_MRTK_banner.png)

[![Build Status](https://waveengineteam.visualstudio.com/Wave.Engine/_apis/build/status/WaveEngine.MixedRealityToolkit-WaveEngine?branchName=master)](https://waveengineteam.visualstudio.com/Wave.Engine/_build/latest?definitionId=61&branchName=master)
[![Nuget](https://img.shields.io/nuget/v/WaveEngine.MRTK?logo=nuget)](https://www.nuget.org/packages/WaveEngine.MRTK)

# What is WaveEngine.MRTK
WaveEngine.MRTK is a project that provides a set of components and features used to accelerate cross-platform XR application development in Wave Engine. 

WaveEngine.MRTK provides a set of **basic building blocks for Wave Engine development on XR platforms** such as
* Microsoft HoloLens 2
* Windows Mixed Reality headsets
* OpenVR headsets (HTC Vive / Oculus Rift)

WaveEngine.MRTK is heavily based on Microsoft's [Mixed Reality Toolkit for Unity](https://github.com/microsoft/MixedRealityToolkit-Unity).<br><br>

# Required software
[![Windows SDK](Documentation/Images/Requirements_WindowsSDK.png)](https://developer.microsoft.com/windows/downloads/windows-10-sdk) [Windows SDK](https://developer.microsoft.com/windows/downloads/windows-10-sdk) | [![Wave Engine](Documentation/Images/Requirements_WaveEngine.png)](https://waveengine.net/Downloads) [Wave Engine](https://waveengine.net/Downloads) | [![Visual Studio 2019](Documentation/Images/Requirements_VisualStudio.png)](http://dev.windows.com/downloads) [Visual Studio 2019](http://dev.windows.com/downloads) | [![Emulators (optional)](Documentation/Images/Requirements_Emulators.png)](https://docs.microsoft.com/windows/mixed-reality/using-the-hololens-emulator) [Emulators (optional)](https://docs.microsoft.com/windows/mixed-reality/using-the-hololens-emulator)|
| :--- | :--- | :--- | :--- |
| You need the Windows 10 SDK. To run apps for immersive headsets, you need the Windows 10 Fall Creators Update | Wave Engine provides support for building XR projects in Windows 10 | Visual Studio is used for code editing, deploying and building application packages | The emulators allow you to test your app without the device in a simulated environment |

# UI and interaction building blocks
These components can be added to your scene and customized using the Wave Engine Editor.
||||
| :--- | :--- | :--- |
| ![Button](Documentation/Images/Button/MRTK_Button_Main.png) [Button](Documentation/README_Button.md) | ![Bounding Box](Documentation/Images/BoundingBox/MRTK_BoundingBox_Main.png) [Bounding Box](Documentation/README_BoundingBox.md) | ![Manipulation Handler](Documentation/Images/ManipulationHandler/MRTK_Manipulation_Main.png) [Manipulation Handler](Documentation/README_ManipulationHandler.md) |
| A button control which supports various input methods, including HoloLens 2's articulated hand | Standard UI for manipulating objects in 3D space | Component for manipulating objects with one or two hands |
| ![Slider](Documentation/Images/Slider/MRTK_UX_Slider_Main.jpg) [Slider](Documentation/README_Slider.md) | ![Hand Menu](Documentation/Images/Solver/MRTK_UX_HandMenu.png) Hand Menu | ![Fingertip Visualization](Documentation/Images/Fingertip/MRTK_FingertipVisualization_Main.png) [Fingertip Visualization](Documentation/README_FingerTip.md) |
| Slider for adjusting values supporting direct hand tracking interaction | Hand-locked UI for quick access, using the Hand Constraint Solver | Visual affordance on the fingertip which improves the confidence for the interaction |
| ![Slate](Documentation/Images/Slate/MRTK_Slate_Main.png) [Slate](Documentation/README_Slate.md) | ![Pointers](Documentation/Images/Pointers/MRTK_Pointer_Main.png) Pointers | ![Voice Command/Dictation](Documentation/Images/VoiceCommands/MRTK_Input_Speech.png) [Voice Command / Dictation](Documentation/README_Voice.md) |
| 2D style plane which supports scrolling with articulated hand input | Learn about various types of pointers | Scripts and examples for integrating speech input|

# Example scene
Check out WaveEngine.MRTK's various types of interactions and UI controls in our sample scene, which can be found in the Releases section.

Video: https://www.youtube.com/watch?v=KbhLifObJNA

![alt Example Scene](Documentation/Images/MRTK_Examples.png)
