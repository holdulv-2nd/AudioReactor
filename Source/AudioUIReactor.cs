using System;
using System.IO;
using FlaxEngine;
using FlaxEngine.GUI;
using NAudio.Wave;

namespace AudioReactor
{
    public class AudioUIReactor : Script
    {
        public enum UIReaction
        {
            LogoPunch,      // Scale UI up/down
            RainbowText,    // Change Text Color
            OpacityFlash    // Fade in/out
        }

        [Header("Controls")]
        public bool IsActive = true;
        public UIReaction Reaction = UIReaction.LogoPunch;

        [Header("Targets")]
        public UIControl TargetUI; // Drag your Label or Image here
        public AudioSource MusicSource;

        [Header("Colors (For Rainbow Text)")]
        public Color[] Colors = { Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Purple };
        public float ColorSpeed = 5.0f;

        [Header("Tuning")]
        public float Sensitivity = 100.0f;
        public float PulsePower = 3.0f;
        public float Smoothing = 10.0f;

        private AudioFileReader _reader;
        private float[] _sampleBuffer;
        private Vector2 _initialSize;
        private int _colorIndex;
        private Color _currentColor;
        private float _lastBeatTime;

        public override void OnStart()
        {
            if (TargetUI == null) TargetUI = Actor.As<UIControl>(); // Try to auto-find
            if (TargetUI != null) _initialSize = TargetUI.Size;
            
            _currentColor = Color.White;

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
            if (!IsActive || TargetUI == null) return;
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
                    float energy = Mathf.Pow(rms, PulsePower) * Sensitivity;

                    if (Reaction == UIReaction.LogoPunch)
                    {
                        // Scale Size
                        Vector2 targetSize = _initialSize + (_initialSize * (energy * 0.005f));
                        TargetUI.Size = Vector2.Lerp(TargetUI.Size, targetSize, Time.DeltaTime * Smoothing);
                        
                        // Keep it centered (Anchor pivot logic)
                        // This assumes pivot is center. If not, it might wiggle.
                    }
                    else if (Reaction == UIReaction.RainbowText)
                    {
                         // Switch color on strong beats
                         if (energy > 50.0f && (Time.UnscaledGameTime - _lastBeatTime) > 0.2f)
                         {
                             _colorIndex = (_colorIndex + 1) % Colors.Length;
                             _lastBeatTime = Time.UnscaledGameTime;
                         }
                         
                         _currentColor = Color.Lerp(_currentColor, Colors[_colorIndex], Time.DeltaTime * ColorSpeed);
                         
                         // Try to apply to Label
                         if (TargetUI.Control is Label label) label.TextColor = _currentColor;
                         // Try to apply to Image
                         else if (TargetUI.Control is Image image) image.Color = _currentColor;
                    }
                    else if (Reaction == UIReaction.OpacityFlash)
                    {
                        float alpha = Mathf.Clamp(energy * 0.02f, 0.2f, 1.0f);
                        TargetUI.Control.Visible = true;
                        
                        // Creating a temp color with modified alpha
                        Color c = TargetUI.Control.BackgroundColor;
                        TargetUI.Control.BackgroundColor = new Color(c.R, c.G, c.B, alpha);
                    }
                }
            }
        }
        public override void OnDestroy() { _reader?.Dispose(); }
    }
}