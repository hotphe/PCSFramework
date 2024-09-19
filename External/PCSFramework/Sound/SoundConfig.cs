using UnityEngine;
using UnityEngine.Audio;
using VInspector;
using System;

namespace PCS.Sound
{
    [CreateAssetMenu(fileName = "SoundConfig", menuName = "Config/SoundConfig")]
    public class SoundConfig : ScriptableObject
    {
        public SoundPlayer SoundPlayerPrefab;
        public bool CollectionCheck = true;
        public int DefaultCapacity = 10;
        public int MaxPoolSize = 100;
        public int MaxSoundInstances = 30;
        public SerializedDictionary<string, SoundData> SoundDataDictionary = new SerializedDictionary<string, SoundData>();
    }

    [Serializable]
    public class SoundData
    {
        public AudioClip Clip;
        public AudioMixerGroup MixerGroup;
        public bool Loop;
        public bool PlayOnAwake;
        public bool FrequentSound;
        public AdvancedSoundData AdvancedSoundData; // Create class to initialize value in inspector.
    }

    [Serializable]
    public class AdvancedSoundData
    {
        public bool Mute;
        public bool BypassEffects;
        public bool BypassListenerEffects;
        public bool BypassReverbZones;
        public int Priority = 128;
        public float Volume = 1f;
        public float Pitch = 1f;
        public float PanStereo;
        public float SpatialBlend;
        public float ReverbZoneMix = 1f;
        public float DopplerLevel = 1f;
        public float Spread;
        public float MinDistance = 1f;
        public float MaxDistance = 500f;
        public bool IgnoreListenerVolume;
        public bool IgnoreListenerPause;
        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;
    }
}
