# Sliders

![Slider example](../Documentation/Images/Slider/MRTK_UX_Slider_Main.jpg)

Sliders are UI components that allow you to continuously change a value by moving a slider on a track. The Pinch Slider can be moved by directly grabbing the slider.


## How to use sliders

**Using ScenePrefab**
Create a new Entity and add the Component ScenePrefab. Select any of the available options in the dropdown list and reload the scene. 

**Create an Slider from scratch**

In order to create an slider you need to create 3 entities: the slider, the slider bar and the thumb
- The **slider** manages the limits and fires the events
- The **bar** is just a mesh showing the bar where slider is moving
- The **thumb** is the 

**Events**
The PinchSlider class has the next events:
- **ValueUpdated**: fired when the slider value is updated.
- **InteractionStarted**: fired when an interaction has been started.
- **InteractionEnded**: fired when an interaction has been ended.

The class SliderChangeColor.cs contains an example showing how to use the slider events

**Adding sounds**
You can add the component **SliderSounds** to any entity using a **PinchSlider ** to add sounds to any of the events mentioned above.
