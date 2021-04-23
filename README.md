# UnityAFrameExporter

Export [A-Frame](https://aframe.io/) From Unity Scene.

Compatible with A-Frame version [1.1.0](https://aframe.io/docs/1.1.0/introduction/)+.

![Export A-Frame scene from Unity 3D](screenshot.gif)

## Quick Start

1. Clone this [repository](https://github.com/primaryobjects/UnityAFrameExporter.git)
2. Copy "AFrameExporter" and "CombineMeshes" Folder to your project from repository
3. Open your project from Unity3D.
4. Open scene you want to export.
5. Click AFrameExporter prefab to show exporter inspector.
6. Click Export button on inspector.
7. Find the `index.html` file in your project Assets folder under `Assets/AFrameExporter/export/index.html`
8. Launch a local web server in the `/export` folder using `python3 -m http.server` to view the result at `http://localhost:8000`

*Note 1, you may need to copy `.png` images into the folder `Assets/AFrameExporter/export/images` for textures used by your scene.*

*Note 2, some models render better using gltf format, instead of obj. You can convert your models using [gltf-exporer](https://github.com/Plattar/gltf-exporter) from within Unity and save to your `/export/models` directory. Finally, edit your exported index.html to load the gltf by changing the `<a-obj-model>` tag to `<a-gltf-model src="models/your_model.gltf"></a-gltf-model>`*

*Note 3, Linux users may need to install the following libraries: `sudo apt install libc6-dev` and `sudo apt install libgdiplus`.*

## Using Sound Effects

To add spatial sound effects to your VR scene, use the following [steps](https://gist.github.com/primaryobjects/66516de4423f302856ecb82f23edb07e#a-frame-audio-sound-in-ios) below.

1. [Download](https://www.freesoundeffects.com/free-sounds/airplane-10004/) sound effects (mp3, wav) and copy to `Assets/AFrameExporter/export/sounds`.
2. In your `index.html` file, add the following section:
    ```html
    <assets>
        <audio id="mysound" src="mysound.mp3" preload="auto"></audio>
    </assets>
    ```
3. To add sound to a specific object, use the following example:
    ```html
    <a-box src="url(images/box.png); audio="src: #mysound; loop: true; distance: 8;"></a-box>
    ```

Sound effects use [Howler.js](https://howlerjs.com/) and are compatible with Chrome, Safari, mobile devices, Android, and Apple iPhone iOS.

### Audio Tag Options

```
loop: boolean, true to play sound effect continuously, default is false
volume: integer, default is 1
distance: integer, how close the camera must be to the object before playing audio, default is 8
fade: integer, how quickly the sound fades when moving away from the object, default is 5000
```

## Export Options

![image1](https://raw.github.com/wiki/umiyuki/UnityAFrameExporter/AFrame4.jpg)

<br>**◆General**<br>
*・Title*<br>
  Title of A-Frame.<br>
*・Library Address*<br>
  A-Frame library address you want to use.<br>
*・Enable_performance_statistics*
  Show Performance Statistics.<br>
**◆Sky**<br>
*・Enable_Sky*
  Enable A-Frame Sky.<br>
*・Sky_color_from_Main Camera_Background*
  Use sky color from Main Camera Background.<br>
*・Sky_color*
  Sky color.<br>
*・Sky_texture*
  Sky texture.<br>
**◆Camera**<br>
*・Wasd_controls_enabled*
  Enable WASD control.<br>
*・Look_controls_enabled*
  Enable Look control.<br>
*・Enable_Sky*
  Enable A-Frame Sky.<br>
*・Cursor_visible*
  Change cursor visible.<br>
*・Cursor_opacity*
  Change cursor opacity.0 to 1.<br>
*・Cursor_scale*
  Change cursor scale.<br>
*・Cursor_color*
  Change cursor color.<br>
*・Cursor_offset*
  Change cursor offset.<br>
*・Cursor_maxdistance*
  Change cursor max distance.<br>
**◆Clear Exported Files**<br>
  Clean exported files.
  If you editted export folder. These file will be deleted.<br>

## Supported Unity3D Objects
**・Main Camera**
Supported parameters are Position, Rotation, Fov, NearClip, FarClip.<br>
**・Light**
Directional, Point, Spot
Supported parameters are Position, Rotaion, Intensity, Color.<br>
**・Single Sprite**
Export as Image.<br>
**・Cube**
Export as Box.
Supported parameters are Scale xyz.<br>
**・Sphere**
Export as Sphere.
Scale parameters are exported average xyz. because A-Frame Sphere have parameter only radius.<br>
**・Cylinder**
Export as cylinder.
Scale y export as height.
Scale xz are exported average for A-Frame cylinder radius.<br>
**・Plane**
Export as plane.<br>
**・Other Meshes**
Export as Obj.
**・Physics Engine**
Using the [aframe-particle-system-component](https://github.com/IdeaSpaceVR/aframe-particle-system-component).
**・Sound Engine**
Using [Howler.js](https://howlerjs.com/) with spacial audio to play sound effects as you approach objects. See [example](https://gist.github.com/primaryobjects/66516de4423f302856ecb82f23edb07e#file-index-html) for how to add to your VR objects.

## Caution

・You can't use Asset from Unity Asset Store limited of EULA.
  But, you can ask asset developer about it.<br>
・If you editted index.html in export folder, and re-export , It will be deleted.
  Before edit A-Frame, please copy export folder to another directory.<br>
・Export only main texture from object.

## Supported Shader Type
・Standard
・Unlit/Color
・Unlit/Texture
・Unlit/Texture Colored
・Legacy Shaders/Transparent/Diffuse
