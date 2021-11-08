# Slate

![Slate](Images/Slate/MRTK_Slate_Main.png)

The Slate prefab offers a thin window style control for displaying 2D content, for example plain text or articles including media

## Using the Slate with Evergine.MRTK

- Create a Plane and configure its material with a texture
- Optional, create a collider with the desired depth
- Add the component HandInteractionPanZoom and configure it
  - Enable Zoom: to enable/disable zoom when using too hands
  - Min zoom: the min amount of zoom allowed
  - Max zoom: the max amount of zoom allowed
  - Lock horizontal: locks the movement horizontally
  - Lock vertical: locks the movement vertically
  - Drag: the amount of drag to apply when releasing the slate with one of the fingers

The slate can be manipulated with bot the finger tips and the hand rays. Hand rays will only work when doing the **_pinch_** gesture
