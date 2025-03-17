# Evergine MRTK Package

![Evergine.MRTK](Documentation/Images/Evergine_MRTK_banner.png)

[![Build Status](https://waveengineteam.visualstudio.com/Wave.Engine/_apis/build/status/Packages/MixedRealityToolkit/MRTK%20CD%20Stable?branchName=master)](https://waveengineteam.visualstudio.com/Wave.Engine/_build/latest?definitionId=97&branchName=master)
[![Nuget](https://img.shields.io/nuget/v/Evergine.MRTK?logo=nuget)](https://www.nuget.org/packages/Evergine.MRTK)

## What is Evergine.MRTK

Evergine.MRTK is an Evergine package that provides a set of components and features used to accelerate cross-platform XR application development in Evergine.

Evergine.MRTK provides a set of **basic building blocks for Evergine development on XR platforms** such as

- Meta Quest devices.
- OpenVR headsets (HTC Vive / Oculus Rift)

Evergine.MRTK is heavily based on Microsoft's [Mixed Reality Toolkit for Unity](https://github.com/microsoft/MixedRealityToolkit-Unity).<br><br>

## Required software

| <a href="https://www.evergine.com"><img src="Documentation/Images/evergine.png" alt="Evergine" width="100"/></a><br/> [Evergine](https://www.evergine.com) | <a href="http://dev.windows.com/downloads"><img src="Documentation/Images/visual_studio.png" alt="Visual Studio" width="100"/></a><br/> [Visual Studio 2019/2022](http://dev.windows.com/downloads) |
|----------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| Evergine provides support for building XR projects in Windows 10                                                                                     | Visual Studio is used for code editing, deploying, and building application packages                                                                            |


## UI and interaction building blocks

These components can be added to your scene and customized using the Evergine Editor.
||||
| :--- | :--- | :--- |
| ![Button](Documentation/Images/Button/MRTK_Button_Main.png) [Button](Documentation/README_Button.md) | ![Bounding Box](Documentation/Images/BoundingBox/MRTK_BoundingBox_Main.png) [Bounding Box](Documentation/README_BoundingBox.md) | ![Manipulation Handler](Documentation/Images/ManipulationHandler/MRTK_Manipulation_Main.png) [Manipulation Handler](Documentation/README_ManipulationHandler.md) |
| A button control which supports various input methods, including HoloLens 2's articulated hand | Standard UI for manipulating objects in 3D space | Component for manipulating objects with one or two hands |
| ![Slider](Documentation/Images/Slider/MRTK_UX_Slider_Main.jpg) [Slider](Documentation/README_Slider.md) | ![Hand Menu](Documentation/Images/Solver/MRTK_UX_HandMenu.png) Hand Menu | ![Fingertip Visualization](Documentation/Images/Fingertip/MRTK_FingertipVisualization_Main.png) [Fingertip Visualization](Documentation/README_FingerTip.md) |
| Slider for adjusting values supporting direct hand tracking interaction | Hand-locked UI for quick access, using the Hand Constraint Solver | Visual affordance on the fingertip which improves the confidence for the interaction |
| ![Slate](Documentation/Images/Slate/MRTK_Slate_Main.png) [Slate](Documentation/README_Slate.md) | ![Pointers](Documentation/Images/Pointers/MRTK_Pointer_Main.png) Pointers | ![Voice Command/Dictation](Documentation/Images/VoiceCommands/MRTK_Input_Speech.png) [Voice Command / Dictation](Documentation/README_Voice.md) |
| 2D style plane which supports scrolling with articulated hand input | Learn about various types of pointers | Scripts and examples for integrating speech input|

## Example scene

Check out Evergine.MRTK's various types of interactions and UI controls in our sample scene, which can be found in the Releases section.

Video: https://www.youtube.com/watch?v=KbhLifObJNA

![alt Example Scene](Documentation/Images/MRTK_Examples.png)


---
Powered by [Evergine](https://evergine.com)

LET'S CONNECT!

- [Youtube](https://www.youtube.com/c/Evergine)
- [Twitter](https://x.com/EvergineTeam)
- [Blog](https://evergine.com/news/)
