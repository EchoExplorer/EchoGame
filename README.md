**Training App for Echolocation**
==============================

# Instructions
## Running the Unity Android App

### Unity Game Engine
If you haven't installed any version of Unity3D, download version 5.6.5:
https://unity3d.com/get-unity/download/archive


Using higher version or MacOS High Sierra may cause problem. Please Unistall the original version and install the 5.6.5 version.


If this step encounters a black screen problem, check this steps to fix:

```
cd ~/Library/Unity/Packages
mkdir -p node_modules/unity-editor-home node_modules/unityeditor-cloud-hub
tar -zxvf unityeditor-cloud-hub-XXX.tgz
mv package/* node_modules/unityeditor-cloud-hub/
tar -zxvf unity-editor-home.XXX.tgz
mv package/* node_modules/unity-editor-home/
rmdir package
```


### Android SDK
Set up the Android SDK for Unity. Follow the instructions from http://docs.unity3d.com/Manual/android-sdksetup.html

### Project
Clone the repository, and open the EchoGrid/ project in Unity
