using System;
using System.IO;
using FlaxEngine;
using NAudio.Wave;

namespace AudioReactor
{
    public class AudioCameraReactor : Script
    {
        [Header("Controls")]
        public bool IsActive = true;

        [Header("Targets")]
        public AudioSource MusicSource;
        public Camera TargetCamera;
        public PostFxVolume TargetPostFx; 

        [Header("Handheld Movement")]
        public float SwaySpeed = 1.0f;      // How fast the "breathing" movement is
        public float SwayAmount = 0.5f;     // How far it sways
        public float ShakeMultiplier = 2.0f; // How hard it shakes on beat
        
        [Header("Camera Zoom (FOV)")]
        public float BaseFOV = 60.0f;
        public float ZoomPunch = 10.0f; 
        public float FOVSmoothing = 15.0f;

        [Header("Blur Effect")]
        public bool EnableBlur = true;
        public float BlurPower = 2.0f; 
        public float BlurSmoothing = 10.0f;

        [Header("Audio Tuning")]
        public float PulsePower = 3.0f;
        public float Sensitivity = 200.0f;

        private AudioFileReader _reader;
        private float[] _sampleBuffer;
        private float _currentFOV;
        private float _currentBlur;
        
        // Motion State
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private float _swayTime;

        public override void OnStart()
        {
            if (TargetCamera != null) 
            {
                BaseFOV = TargetCamera.FieldOfView;
                // We capture the starting spot so we sway relative to it
                _initialPosition = TargetCamera.LocalPosition;
                _initialRotation = TargetCamera.LocalOrientation;
            }
            
            _currentFOV = BaseFOV;

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
            // 🛑 RESET IF DISABLED
            if (!IsActive)
            {
                if (TargetCamera != null)
                {
                    TargetCamera.FieldOfView = Mathf.Lerp(TargetCamera.FieldOfView, BaseFOV, Time.DeltaTime * 2.0f);
                    TargetCamera.LocalOrientation = Quaternion.Slerp(TargetCamera.LocalOrientation, _initialRotation, Time.DeltaTime * 2.0f);
                }
                return;
            }

            if (_reader == null) return;

            // Update Sway Time (always moving even between beats)
            _swayTime += Time.DeltaTime * SwaySpeed;

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
                    float beatEnergy = Mathf.Pow(rms, PulsePower) * Sensitivity;

                    // --- 1. Camera FOV ---
                    if (TargetCamera != null)
                    {
                        float targetFOV = BaseFOV - (beatEnergy * ZoomPunch); 
                        _currentFOV = Mathf.Lerp(_currentFOV, targetFOV, Time.DeltaTime * FOVSmoothing);
                        TargetCamera.FieldOfView = _currentFOV;

                        // --- 2. Handheld Sway + Beat Shake ---
                        // A. Sway: Gentle Figure-8 pattern
                        float swayX = Mathf.Cos(_swayTime) * SwayAmount;
                        float swayY = Mathf.Sin(_swayTime * 2.0f) * SwayAmount;

                        // B. Shake: Random jitter based on beat energy
                        // We use a random value scaled by how loud the music is
                        float shakeX = (new Random().NextFloat() - 0.5f) * beatEnergy * ShakeMultiplier;
                        float shakeY = (new Random().NextFloat() - 0.5f) * beatEnergy * ShakeMultiplier;
                        float shakeRoll = (new Random().NextFloat() - 0.5f) * beatEnergy * ShakeMultiplier * 0.5f;

                        // Apply Rotation (Original + Sway + Shake)
                        Quaternion swayRot = Quaternion.Euler(swayY + shakeY, swayX + shakeX, shakeRoll);
                        TargetCamera.LocalOrientation = Quaternion.Slerp(TargetCamera.LocalOrientation, _initialRotation * swayRot, Time.DeltaTime * 10.0f);
                    }

                    // --- 3. Blur ---
                    if (TargetPostFx != null && EnableBlur)
                    {
                        float targetBlur = beatEnergy * BlurPower;
                        _currentBlur = Mathf.Lerp(_currentBlur, targetBlur, Time.DeltaTime * BlurSmoothing);

                        var dof = TargetPostFx.DepthOfField;
                        dof.Enabled = true;
                        dof.BlurStrength = Mathf.Clamp(_currentBlur, 0.0f, 1.0f); 
                        dof.FocalDistance = 0; 
                        dof.NearTransitionRange = 0;
                        dof.FarTransitionRange = 10000; 
                        TargetPostFx.DepthOfField = dof;
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