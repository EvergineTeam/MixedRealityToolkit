# Button

![Button](../Documentation/Images/Button/MRTK_Button_Main.png)

A button gives the user a way to trigger an immediate action. It is one of the most foundational components in mixed reality. MRTK provides various types of button prefabs.

## Button prefabs in WaveEngine.MRTK

Examples of the button prefabs under ``Scenes/Prefabs`` folder

### Collider based buttons

|  ![PressableButtonHoloLens2](../Documentation/Images/Button/MRTK_Button_Prefabs_HoloLens2.png) PressableButtonHoloLens2 | ![PressableButtonHoloLens2Unplated](../Documentation/Images/Button/MRTK_Button_Prefabs_HoloLens2Unplated.png) PressableButtonHoloLens2Unplated | ![PressableButtonHoloLens2Circular](../Documentation/Images/Button/MRTK_Button_Round.png) PressableRoundButton |
|:--- | :--- | :--- |
| HoloLens 2's shell-style button with backplate | HoloLens 2's shell-style button without backplate  | Round shape push button |

## How to use pressable buttons

####Using ScenePrefab

Create a new Entity and add the Component ScenePrefab. Select any of the next options in the dropdown list and reload the scene:
- PressableRoundButton
- PressableButtonPlated32x32
- PressableButtonUnplated

These button prefabs are already configured to have audio-visual feedback. The Entity can be scaled to get different button sizes. The size of the prefabs is 32x32mm.

