n

# 🎵 AudioReactor for Flax Engine

**AudioReactor** is a plug-and-play real-time audio visualization plugin for Flax Engine. It bridges the **NAudio** library with Flax, allowing you to drive lights, materials, and gameplay events using audio frequencies (RMS/FFT) from your game music.

## ⚡ The Problem & The Solution
Flax Engine automatically imports `.wav` files as optimized `.flax` assets, which are great for playback but inaccessible to external analysis libraries like NAudio. 

**AudioReactor solves this automatically:**
1. It intercepts `.wav` imports in the Editor.
2. It creates a hidden **"Safe House"** folder (`/MusicData`) in your project root.
3. It keeps a raw copy of the audio there for analysis, while Flax uses the optimized asset for playback.
4. **Result:** You get perfect syncing without any manual file management.

---

## 📦 Installation
1. Download the latest **Release** (or clone this repo).
2. Copy the `AudioReactor` folder into your project's `Plugins/` directory:
MyProject/ ├── Content/ ├── Source/ └── Plugins/ └── AudioReactor/ <-- Put it here

3. Open your project. The plugin handles the rest.

---

## 🚀 How to Use

### 1. Import Your Music
* Drag a `.wav` file into the Flax **Content Window**.
* *Magic happens:* The plugin silently copies the raw file to the `MusicData` folder.

### 2. Set Up Audio
* Add an **Audio Source** actor to your scene.
* Assign your imported music clip to it.

### 3. Add the Reactor
* Add the `RealTimePulse` script to any actor (e.g., a Light or a Sphere).
* **Music Source:** Drag your Audio Source actor here.
* **Glow Lights:** Drag the Point Lights you want to animate here.

### 4. Play
* Hit **F5**. The lights will pulse to the beat.

---

## ⚙️ Tuning
* **Sensitivity:** Adjusts the overall brightness response.
* **PulsePower:** Exponential scaling. Higher values (e.g., 3.0) create snappy, strobe-like beats. Lower values (e.g., 1.0) create smooth breathing.
* **SmoothSpeed:** How fast the light fades out after a beat.

---

## ⚖️ License & Credits
* **AudioReactor** is created by **Holdulv Music** (Fiana).
* This plugin uses [NAudio](https://github.com/naudio/NAudio) (c) Mark Heath, under the **Ms-PL License**.
