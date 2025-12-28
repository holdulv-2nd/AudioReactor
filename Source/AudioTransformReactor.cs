using System;
using System.IO;
using FlaxEngine;
using NAudio.Wave;

namespace AudioReactor
{
    public class AudioTransformReactor : Script
    {
        public enum ReactionMode
        {
            ScalePulse,     // Gets big on beat (Speakers, Bass)
            SpinOnBeat,     // Rotates faster on beat
            PositionJitter, // Shakes in place (Glitch effect)
            RandomTeleport  // Jumps to a new spot (Chaotic)
        }

        [Header("Controls")]
        public bool IsActive = true;
        public ReactionMode Mode = ReactionMode.ScalePulse;

        [Header("Audio Setup")]
        public AudioSource MusicSource;

        [Header("Tuning")]
        public float Sensitivity = 150.0f; 
        public float PulsePower = 3.0f;
        public float Smoothing = 15.0f;

        [Header("Mode Settings")]
        public Vector3 Multiplier = Vector3.One; // x,y,z intensity
        [Tooltip("For Teleport: How far can it jump?")]
        public float TeleportRadius = 50.0f;
        
        private AudioFileReader _reader;
        private float[] _sampleBuffer;
        
        private Vector3 _initialScale;
        private Vector3 _initialPos;
        private Quaternion _initialRot;
        private Vector3 _targetScale;
        private Vector3 _currentJitter;

        public override void OnStart()
        {
            _initialScale = Actor.LocalScale;
            _initialPos = Actor.LocalPosition;
            _initialRot = Actor.LocalOrientation;
            _targetScale = _initialScale;

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
            if (!IsActive)
            {
                // Reset to boring state
                Actor.LocalScale = Vector3.Lerp(Actor.LocalScale, _initialScale, Time.DeltaTime * 5.0f);
                Actor.LocalPosition = Vector3.Lerp(Actor.LocalPosition, _initialPos, Time.DeltaTime * 5.0f);
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
                    float energy = Mathf.Pow(rms, PulsePower) * Sensitivity;

                    switch (Mode)
                    {
                        case ReactionMode.ScalePulse:
                            // Pop size based on beat
                            Vector3 target = _initialScale + (Multiplier * energy * 0.01f);
                            Actor.LocalScale = Vector3.Lerp(Actor.LocalScale, target, Time.DeltaTime * Smoothing);
                            break;

                        case ReactionMode.SpinOnBeat:
                            // Spin faster when loud
                            float rotSpeed = energy * Multiplier.Y; // Use Y for speed
                            Actor.LocalOrientation *= Quaternion.Euler(0, rotSpeed * Time.DeltaTime, 0);
                            break;

                        case ReactionMode.PositionJitter:
                            // Glitch movement
                            if (energy > 1.0f) // Only shake if loud enough
                            {
                                float x = (new Random().NextFloat() - 0.5f) * Multiplier.X * energy * 0.1f;
                                float y = (new Random().NextFloat() - 0.5f) * Multiplier.Y * energy * 0.1f;
                                float z = (new Random().NextFloat() - 0.5f) * Multiplier.Z * energy * 0.1f;
                                Actor.LocalPosition = Vector3.Lerp(Actor.LocalPosition, _initialPos + new Vector3(x,y,z), Time.DeltaTime * Smoothing);
                            }
                            else
                            {
                                Actor.LocalPosition = Vector3.Lerp(Actor.LocalPosition, _initialPos, Time.DeltaTime * 5.0f);
                            }
                            break;

                        case ReactionMode.RandomTeleport:
                            // If a huge beat hits, move to random spot
                            if (energy > 50.0f) // High threshold
                            {
                                float x = (new Random().NextFloat() - 0.5f) * TeleportRadius;
                                float z = (new Random().NextFloat() - 0.5f) * TeleportRadius;
                                Actor.LocalPosition = _initialPos + new Vector3(x, 0, z);
                            }
                            break;
                    }
                }
            }
        }
        
        public override void OnDestroy() { _reader?.Dispose(); }
    }
}