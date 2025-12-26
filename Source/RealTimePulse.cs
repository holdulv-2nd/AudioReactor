using System;
using System.IO;
using FlaxEngine;
using NAudio.Wave;

namespace AudioReactor // 👈 Added Namespace
{
    public class RealTimePulse : Script
    {
        [Header("Setup")]
        public AudioSource MusicSource;
        public PointLight[] GlowLights;

        [Header("Tuning")]
        public float Sensitivity = 200.0f; 
        public float PulsePower = 3.0f;
        public float SmoothSpeed = 20.0f;
        public float MinBrightness = 5.0f;

        private AudioFileReader _reader;
        private float[] _sampleBuffer;
        private float _currentBrightness;

        public override void OnStart()
        {
            if (MusicSource == null || MusicSource.Clip == null) return;

            // 1. Get filename
            string filename = Path.GetFileName(MusicSource.Clip.Path); 
            filename = Path.ChangeExtension(filename, ".wav");

            // 2. Look in "MusicData" (Project Root)
            string safeHousePath = Path.Combine(Globals.ProjectFolder, "MusicData", filename);

            if (File.Exists(safeHousePath))
            {
                _reader = new AudioFileReader(safeHousePath);
                _sampleBuffer = new float[1024]; 
            }
        }

        public override void OnUpdate()
        {
            if (_reader == null || MusicSource == null) return;

            float engineTime = MusicSource.Time;
            long targetPosition = (long)(engineTime * _reader.WaveFormat.AverageBytesPerSecond);
            
            if (targetPosition < _reader.Length)
            {
                _reader.Position = targetPosition;
                int samplesRead = _reader.Read(_sampleBuffer, 0, _sampleBuffer.Length);

                if (samplesRead > 0)
                {
                    float sum = 0;
                    for (int i = 0; i < samplesRead; i++)
                    {
                        float sample = _sampleBuffer[i];
                        sum += sample * sample; 
                    }
                    float rms = (float)Math.Sqrt(sum / samplesRead);
                    float contrast = Mathf.Pow(rms, PulsePower);
                    float targetBrightness = (contrast * Sensitivity) + MinBrightness;

                    _currentBrightness = Mathf.Lerp(_currentBrightness, targetBrightness, Time.DeltaTime * SmoothSpeed);

                    if (GlowLights != null)
                    {
                        foreach (var light in GlowLights)
                        {
                            if (light != null) light.Brightness = _currentBrightness;
                        }
                    }
                }
            }
        }
        
        public override void OnDestroy()
        {
            _reader?.Dispose();
        }
    }
}