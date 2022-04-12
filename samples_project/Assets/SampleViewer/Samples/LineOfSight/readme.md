# Line of sight

Show a line of sight between two objects. Check if the line of sight is obstructed by the ArcGIS 3D model layer.

![Image of line of sight](LineOfSight.jpg)

## How it works

1. Check the box for "Use Mesh Colliders" on the `Arc GIS Map View` component.
2. Create a parent object with an `Arc GIS Location` Component.
3. Create child game objects in the scene that use `Transform` components.
4. Move an object around the scene so that it comes in and out of the "view" of another object.
5. Check for any obstructions between objects using the `Physics.Raycast` method.
6. Use the `RaycastHit.point` property to determine where the line of sight from the first object collides.

## Tags

line of sight, raycast, visibility, visibility analysis
