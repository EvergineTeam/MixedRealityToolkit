# Bounding Box
![BoundingBox](../Documentation/Images/BoundingBox/MRTK_BoundingBox_Main.png)

The BoundingBox.cs script provides basic functionality for transforming objects in mixed reality. A bounding box will show a cube around the hologram to indicate that it can be interacted with. Handles on the corners and edges of the cube allow scaling or rotating the object. All interactions and visuals can be easily customized.

## How to use the BoundingBox component

- Add the component BoundingBox to the object that you want to be manipulated
- Add the proper materials
    - **BoxMaterial**: the material that shows the bounding box itself
    - **BoxGrabbedMaterial**: the material that shows the bounding box when it is being grabbed
    - **WireframeMaterial**: used for the edges
    - **HandleMaterial**: used for the handles
    - **HandleGrabbedMaterial**: used for the handles when they are bing grabbed
- Addjust the handles that you want to be visible:
    - **Show wireframe**: wether to display the edges or not
    - **Show scale handles**: wether to display the uniform scale handles (in the corners) or not
    - **Show X/Y/Z scale handle**: wether to display the axis scale handles or not
    - **Show X/Y/Z rotation handles**: wether to display the axis rotation handles or not

    It is also recommended to add a SimpleManipulationHandler with constraints in rotations and scales to manage grabbing the bounding box