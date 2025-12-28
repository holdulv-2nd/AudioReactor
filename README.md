# 🎵 AudioReactor for Flax Engine

Real-time audio visualization for Flax Engine. This plugin analyzes audio frequencies using NAudio and syncs your game world (lights, cameras, UI, objects) to the beat.

## ✨ Features
* **Plug & Play:** Just drag it in, no C# knowledge required.
* **Auto-Sync:** Handles the complex math to sync game time with audio time.
* **The "Safe House":** Automatically manages raw .wav files in a `MusicData` folder so Flax doesn't corrupt them.
* **Modular Reactors:** Different scripts for different needs (Camera, UI, Objects, Lights).

## 📦 File Structure
Ensure your files are arranged like this:
AudioReactor/ ├── AudioReactor.flaxproj ├── README.md └── 
Source/ ├── NAudio.dll ├── AudioReactorPlugin.cs ├── AudioReactor.Build.cs ├── RealTimePulse.cs (Lights & Materials) ├── AudioCameraReactor.cs (FOV, Sway, Shake) ├── AudioTransformReactor.cs (Scale, Spin, Jitter) └── AudioUIReactor.cs (UI Punch, Color)


## 🚀 How to Use

### 1. Setup
1. Drag the `AudioReactor` folder into your project's `Plugins/` folder.
2. Open your project. The plugin will activate automatically.
3. Drag a `.wav` file into your Content window.
   * *The plugin will silently copy a raw version to `MusicData/` for analysis.*

### 2. Available Reactors

#### 💡 RealTimePulse (Lights & Materials)
Controls Point Lights and Emissive Materials.
* **Rainbow Mode:** Cycles through colors on the beat.
* **PulsePower:** Controls "snappiness." Higher = strobing.
* **Usage:** Drag your `PointLight` or `StaticModel` into the slots.

#### 🎥 AudioCameraReactor (Cinematics)
Makes the camera dance to the music.
* **ZoomPunch:** Pulls the camera FOV in on bass hits.
* **Handheld Sway:** Adds a natural "breathing" motion.
* **ShakeMultiplier:** How violently the camera shakes on loud beats.
* **Usage:** Attach to your Scene Camera actor.

#### 🧊 AudioTransformReactor (Objects)
Physical movement for meshes.
* **Modes:**
    * `ScalePulse`: Bounces like a speaker.
    * `SpinOnBeat`: Rotates faster when music is loud.
    * `PositionJitter`: Glitch effect.
    * `RandomTeleport`: Chaos mode (use with caution!).

#### 🖼️ AudioUIReactor (Interface)
Syncs UI elements.
* **LogoPunch:** Scales an image to the beat.
* **RainbowText:** Changes text color on beat.
* **Usage:** Attach to a UI Control (Label or Image).

---
*Built with ❤️ by HoldulV Music using NAudio.*
