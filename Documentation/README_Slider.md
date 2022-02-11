# Sliders

![Slider example](../Documentation/Images/Slider/MRTK_UX_Slider_Main.jpg)

Sliders are UI components that allow you to continuously change a value by moving a slider on a track. The Pinch Slider can be moved by directly grabbing the slider.


## How to use sliders

### Using prefabs

Drag one of the default MRTK slider prefabs onto your scene.

### Create an Slider from scratch

In order to create an slider you need to create 3 entities: the slider, the slider bar and the thumb

- The **slider** manages the limits and fires the events
    - Create an empty Entity
    - Add the **PinchSlider** Component

- The **bar** is just a mesh showing the bar where slider is moving
    - Create an Entity as child of the **slider** (a cylinder for example)
    - Scale it to fit your needs (usually scaling it a lot on the y-axis)

- The **thumb** is the object that the user grabs
    - Create another Entity as child of the **slider** and siblig to the **bar** (a box for example)
    - Add BoxCollider
    - Add a StaticBody3D
    - Add a NeearInteractionGrabable

Finally adjust the limits of the **slider** to make them fit the **bar**

## Events

The PinchSlider class has the next events:
- **ValueUpdated**: fired when the slider value is updated.
- **InteractionStarted**: fired when an interaction has been started.
- **InteractionEnded**: fired when an interaction has been ended.

The class SliderChangeColor.cs contains an example showing how to use the slider events

## Adding sounds

You can add the component **SliderSounds** to any entity using a **PinchSlider** to add sounds to any of the events mentioned above.
