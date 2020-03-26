# Manipulation handler

![Manipulation handler](../Documentation/Images/ManipulationHandler/MRTK_Manipulation_Main.png)

The *SimpleManipulationHandler* script allows for an object to be made movable, scalable, and rotatable using one or two hands. 

## How to use the SimpleManipulationHandler class

Follow the next steps in order to create a SimpleManipulationHandler from scratch

- Add the object you want to interact with in your scene
- Add a **collider** (a BoxCollider for example)
- Add a **StaticBody3D**
- Finally add the **SimpleManipulationHandler** to it

You can now interact with the object

### Adding physics

Objects with the SimpleManipulationHandler script can have physics applied to them. You just need to follow the steps above but instead of adding a **StaticBody3D**, replace it with a **RigidBody3D**

### Events

The SimpleManipulationHandler class has the next events:

- ManipulationStarted: fired when the object starts being manipulated
- ManipulationEnded: fired when the object end being manipulated

### Adding Sounds

Sounds can be added to the **SimpleManipulationHandler** adding the class **ManipulationHandlerSoundFeedbackComponent**. You can add sounds to any of the events mentioned above

