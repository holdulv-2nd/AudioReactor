# 🎵 AudioReactor for Flax Engine

Real-time audio visualization for Flax Engine. 
This plugin analyzes audio frequencies using NAudio and syncs your game lights to the beat.

## ✨ Features
* **Plug & Play:** Just drag it in, no C# knowledge required.
* **Auto-Sync:** Handles the complex math to sync game time with audio time.
* **The "Safe House":** Automatically manages raw .wav files in a `MusicData` folder so Flax doesn't corrupt them.
* **Zero-Config:** Intercepts audio imports automatically.

## 📦 Installation
1. Download the `AudioReactor` folder.
2. Drag it into your project's `Plugins/` folder.
3. Open your project. The plugin will activate automatically.

## 🚀 How to Use
1. **Import Audio:** Drag a `.wav` file into your Content window. 
   * *The plugin will silently copy a raw version to `MusicData/` for analysis.*
2. **Setup Scene:** Add an **Audio Source** to your scene and assign the music clip.
3. **Add Lights:** Create some Point Lights.
4. **Add Script:** Add the `RealTimePulse` script to any actor.
   * Drag your **Audio Source** into the script slot.
   * Drag your **Point Lights** into the Lights array.
5. **Hit Play (F5)!** ## ⚙️ Tuning
* **Sensitivity:** Adjusts how bright the lights get.
* **PulsePower:** Controls the "snappiness." Higher values = sharper beats (strobe effect).
* **SmoothSpeed:** Controls the fade out.

---
*Built with ❤️ by HoldulV Music using NAudio.*