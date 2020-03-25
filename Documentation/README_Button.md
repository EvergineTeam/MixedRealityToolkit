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

**Using ScenePrefab**

Create a new Entity and add the Component ScenePrefab. Select any of the next options in the dropdown list and reload the scene:
- **PressableRoundButton**
- **PressableButtonPlated32x32**
- **PressableButtonUnplated**

These button prefabs are already configured to have audio-visual feedback. The Entity can be scaled to get different button sizes. The size of the prefabs is 32x32mm.

**Create a button from scratch**

You need to create two entities: the **button** and the **visual feedback**
 - The **button** manages the collisions and fires the events. This is usually the base of the button and a collider defining the area of interaction
    - Create an empty **Entity**
    - Add a **BoxCollider** and **StaticBody3D** to configure the collisions
    - Add a **NearInteractionTouchable** component
    - Optional, you can add a mesh to the button either in this node or as a child
    - Finally add the **PressableButton** component
 - The **visual feedback** is the entity that shows the current state of the button. This is the part of the button that moves or changes its color when the button is pressed
    - Add an empty **Entity** as child of the button
    - Add the component **PressableButtonVisualFeedbackComponent**. This component allows you to
      - Change the color of the mesh when the button is pressed or released
      - Compress the mesh, scaling its transform


**Button events**

The pressable button class has the next events
- **ButtonPressed**: fired when the button is pressed.
- **ButtonReleased**: fired when the button is released.

You can find examples of how to use this events in the class ColorChanger.cs

**Adding sounds**

You can add the component **PressableButtonSoundFeedbackComponent** to any entity using a PressableButton to add sounds when it's pressed or relaesed

Take a look at the piano in the demo scene for a good example of how to add sounds to buttons
