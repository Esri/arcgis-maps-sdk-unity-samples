# Explore the world in tabletop XR

Control a Virtual Table Top in Virtual Reality or Augmented Reality
![Image of XR Table Top](XRTableTop.png)

## How to use the sample

There are two ways to use this headset, Tethered (Desktop) and Standalone (Android). This sample utilizes the OpenXR framework and supports the following headsets/devices for **Desktop XR**:
- Meta Rift
- Meta Rift S
- Meta Quest
- Meta Quest 2
- Meta Quest Pro
- Meta Quest 3
- HTC Vive
- HTC Vive Pro
- HTC Vive Cosmos
- Valve Index

This sample utilizes the OpenXR framework and supports the following headsets/devices for **Android XR**:
- Meta Quest
- Meta Quest 2
- Meta Quest Pro
- Meta Quest 3
- HTC Vive XR Elite

### Deployment

For deploying, please refer to this [document](https://developers.arcgis.com/unity/deployment/)
In order for the sample to run on Meta Headsets, the device must be placed in **Developer Mode**. Please refer to this [document](https://developer.oculus.com/documentation/native/android/mobile-intro/) for how to perform such an action. This must be done for running the sample in the Unity Engine, making a build to run on Windows, or making a build directly to the headset (Meta Quest, Meta Quest 2, and Meta Quest Pro)

Please ensure that the project is using URP if you are using any of the Meta Headsets listed above. HDRP may be used for any of the other headsets, but not Meta. It will cause errors in the project. 

**Note: AR Foundation is installed and used to allow Passthrough on the Meta Quest Devices. If you would like to have Passthrough on other devices, please refer to the developer section for that headset**
**For Android Use**
1. Ensure your headset is in developer mode and that unknown sources are allowed on your device.
2. Ensure your headset is connected properly to your computer.
3. Navigate to File > Build Settings within Unity.
4. Ensure that your Build Target is set to Android, if it is not please change it by selecting, switch platform after clicking on 'Android'. If you do not have the option to change it, close Unity and install the Android Build Support to your Unity Editor through Unity Hub.
5. Inside of the build settings, select your headset under 'Run Device'.
6. Click 'Build and Run'.
7. Once finished, you may put your headset on and enjoy the sample.

**Using the Application**
1. Ensure your headset is properly connected to your computer. For Meta Headsets, make sure the Meta Desktop App is open and your headset says **connected** in the **devices** section. For Steam VR headsets, ensure Steam is open and Steam VR is running. 
2. Place the headset on your head. The sample should automatically use the headset to control the camera's position and rotation in the world. 
3. Using the right joystick you can zoom in or out of the map in order to get a different perspective.
4. Pressing the grab buttons on either controller and aiming at the 'Table Handle' will allow you to move the entire application around in virtual space.
5. Pressing the index trigger and aiming at the map, will allow you to pan the map in order to get a different perspective.
6. Interacting with the menu for cycling locations, or the zoom-in and zoom-out buttons next to the table handle can be done by aiming at the respective button and pressing the index trigger button.

## How it works

This sample is built off the Tabletop Map Sample provided with the plugin. Once the plugin is installed, select samples and click import.

1. Navigate to Assets > SampleViewer > Samples > XRTableTop and drag the Map Prefab into the scene.
2. Drag and Drop the **XR Interaction Hands** Prefab into the scene.
3. Drag the Table Handle Prefab into the scene.
4. In the Hierarchy window, click on the Table Handle Prefab and expand it. Set the Map transform as the follow transform within the Table Handle.

## Tags

Exploration, First Person, Virtual Reality
