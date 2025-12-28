using System;
using System.IO;
using System.Collections.Generic;
using FlaxEngine;
using NAudio.Wave;

namespace AudioReactor
{
    public class RealTimePulse : Script
    {
        [Header("Controls")]
        public bool IsActive = true; // 👈 The Master Switch

        [Header("Audio Setup")]
        public AudioSource MusicSource;

        [Header("Light Reactors")]
        public PointLight[] GlowLights;

        [Header("Mesh Reactors (Bloom/Emissive)")]
        public StaticModel[] GlowMeshes;
        public string MaterialParameter = "Emissive";

        [Header("Color Tuning")]
        public bool RainbowMode = false;
        public Color BaseColor = Color.White;
        public Color[] BeatColors;
        public float ColorChangeSpeed = 5.0f;

        [Header("Tuning")]
        public float Sensitivity = 200.0f; 
        public float PulsePower = 3.0f;     
        public float SmoothSpeed = 20.0f;
        public float BeatThreshold = 0.5f; 
        public float MinBrightness = 1.0f;

        private AudioFileReader _reader;
        private float[] _sampleBuffer;
        private float _currentBrightness;
        private List<MaterialInstance> _dynamicMats = new List<MaterialInstance>();
        
        // Color State
        private int _colorIndex = 0;
        private Color _targetColor;
        private Color _currentColor;
        private float _lastBeatTime = 0.0f;

        public override void OnStart()
        {
            _targetColor = BaseColor;
            _currentColor = BaseColor;
            _currentBrightness = MinBrightness;

            if (GlowMeshes != null && GlowMeshes.Length > 0)
            {
                foreach (var mesh in GlowMeshes)
                {
                    if (mesh != null)
                    {
                        var mat = mesh.CreateDynamicMaterialInstance(0);
                        mesh.SetMaterial(0, mat);
                        _dynamicMats.Add(mat);
                    }
                }
            }

            if (MusicSource == null || MusicSource.Clip == null) return;

            string filename = Path.GetFileName(MusicSource.Clip.Path); 
            filename = Path.ChangeExtension(filename, ".wav");
            string safeHousePath = Path.Combine(Globals.ProjectFolder, "MusicData", filename);

            if (File.Exists(safeHousePath))
            {
                _reader = new AudioFileReader(safeHousePath);
                _sampleBuffer = new float[1024]; 
            }
        }

        public override void OnUpdate()
        {
            // 🛑 MASTER SWITCH LOGIC
            if (!IsActive)
            {
                // Smoothly return to "Off" state
                _currentBrightness = Mathf.Lerp(_currentBrightness, MinBrightness, Time.DeltaTime * 5.0f);
                _currentColor = Color.Lerp(_currentColor, BaseColor, Time.DeltaTime * 5.0f);
                ApplyEffects();
                return;
            }

            if (_reader == null) return;

            float engineTime = MusicSource.Time;
            long targetPosition = (long)(engineTime * _reader.WaveFormat.AverageBytesPerSecond);
            
            if (targetPosition < _reader.Length)
            {
                _reader.Position = targetPosition;
                int samplesRead = _reader.Read(_sampleBuffer, 0, _sampleBuffer.Length);

                if (samplesRead > 0)
                {
                    float sum = 0;
                    for (int i = 0; i < samplesRead; i++) sum += _sampleBuffer[i] * _sampleBuffer[i];
                    float rms = (float)Math.Sqrt(sum / samplesRead);

                    // Color Logic
                    if (RainbowMode && BeatColors != null && BeatColors.Length > 0)
                    {
                        if (rms > BeatThreshold && (Time.UnscaledGameTime - _lastBeatTime) > 0.1f)
                        {
                            _colorIndex = (_colorIndex + 1) % BeatColors.Length;
                            _targetColor = BeatColors[_colorIndex];
                            _lastBeatTime = Time.UnscaledGameTime;
                        }
                        _currentColor = Color.Lerp(_currentColor, _targetColor, Time.DeltaTime * ColorChangeSpeed);
                    }
                    else
                    {
                        _currentColor = BaseColor;
                    }

                    // Brightness Logic
                    float contrast = Mathf.Pow(rms, PulsePower);
                    float targetBrightness = (contrast * Sensitivity) + MinBrightness;
                    _currentBrightness = Mathf.Lerp(_currentBrightness, targetBrightness, Time.DeltaTime * SmoothSpeed);

                    ApplyEffects();
                }
            }
        }

        private void ApplyEffects()
        {
            // Apply to Lights
            if (GlowLights != null)
            {
                foreach (var light in GlowLights)
                {
                    if (light != null)
                    {
                        light.Brightness = _currentBrightness;
                        light.Color = _currentColor;
                    }
                }
            }

            // Apply to Meshes
            if (_dynamicMats.Count > 0)
            {
                Color finalColor = _currentColor * _currentBrightness;
                foreach (var mat in _dynamicMats)
                {
                    mat.SetParameterValue(MaterialParameter, finalColor);
                }
            }
        }
        
        public override void OnDestroy()
        {
            _reader?.Dispose();
        }
    }
}
