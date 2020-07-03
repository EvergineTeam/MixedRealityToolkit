# Manipulation handler

![Manipulation handler](../Documentation/Images/ManipulationHandler/MRTK_Manipulation_Main.png)

The *SimpleManipulationHandler* script allows for an object to be made movable, scalable, and rotatable using one or two hands. 

## How to use the SimpleManipulationHandler class

Follow the next steps in order to create a SimpleManipulationHandler from scratch

- Add the object you want to interact with in your scene
- Add the **SimpleManipulationHandler** to it
- [Optional] Add a **collider** (a BoxCollider for example). If the collider doesn't exist it the program will try to find it on any of its children, if it's not found then a default one will be created
- [Optional] Add a **StaticBody3D**

You can now interact with the object

### Adding physics

Objects with the SimpleManipulationHandler script can have physics applied to them. You just need to follow the steps above but instead of adding a **StaticBody3D**, replace it with a **RigidBody3D**

### Constraints
By default the SimpleManipulationHandler will allow the user to move, rotate and scale the target object. Constraints can be added to avoid manipulations of these operations:
- ConstraintPosX, ConstraintPosY, ConstraintPosZ will constraint the movement in any of the 3 axis
- ConstraintRotX, ConstraintRotY, ConstraintRotZ will constraint the rotation in any of the 3 axis
- ConstraintScaleX, ConstraintScaleY, ConstraintScaleZ will constraint the scale in any of the 3 axis

### Events

The SimpleManipulationHandler class has the next events:

- ManipulationStarted: fired when the object starts being manipulated
- ManipulationEnded: fired when the object end being manipulated

### Adding Sounds

Sounds can be added to the **SimpleManipulationHandler** adding the class **ManipulationHandlerSoundFeedbackComponent**. You can add sounds to any of the events mentioned above

