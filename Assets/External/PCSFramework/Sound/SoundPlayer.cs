using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Generic;

namespace PCS.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundPlayer : MonoBehaviour
    {
        private AudioSource _audioSource;
        private CancellationTokenSource cts;
        public LinkedListNode<SoundPlayer> Node { get; set; }
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (!_audioSource)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        public void Initialize(SoundData data)
        {
            _audioSource.clip = data.Clip;
            _audioSource.outputAudioMixerGroup = data.MixerGroup;
            _audioSource.loop = data.Loop;
            _audioSource.playOnAwake = data.PlayOnAwake;
           
            _audioSource.mute = data.AdvancedSoundData.Mute;
            _audioSource.bypassEffects = data.AdvancedSoundData.BypassEffects;
            _audioSource.bypassListenerEffects = data.AdvancedSoundData.BypassListenerEffects;
            _audioSource.bypassReverbZones = data.AdvancedSoundData.BypassReverbZones;
            
            _audioSource.priority = data.AdvancedSoundData.Priority;
            _audioSource.volume = data.AdvancedSoundData.Volume;
            _audioSource.pitch = data.AdvancedSoundData.Pitch;
            _audioSource.panStereo = data.AdvancedSoundData.PanStereo;
            _audioSource.spatialBlend = data.AdvancedSoundData.SpatialBlend;
            _audioSource.reverbZoneMix = data.AdvancedSoundData.ReverbZoneMix;
            _audioSource.dopplerLevel = data.AdvancedSoundData.DopplerLevel;
            _audioSource.spread = data.AdvancedSoundData.Spread;
            
            _audioSource.minDistance = data.AdvancedSoundData.MinDistance;
            _audioSource.maxDistance = data.AdvancedSoundData.MaxDistance;
            
            _audioSource.ignoreListenerVolume = data.AdvancedSoundData.IgnoreListenerVolume;
            _audioSource.ignoreListenerPause = data.AdvancedSoundData.IgnoreListenerPause;
            
            _audioSource.rolloffMode = data.AdvancedSoundData.RolloffMode;
        }

        public void Play()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }

            cts = new CancellationTokenSource();
            PlaySound(cts.Token).Forget();
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
            _audioSource.Stop();
            SoundManager.Instance.ReturnToPool(this);
        }

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            _audioSource.pitch += UnityEngine.Random.Range(min, max);
        }

        private async UniTaskVoid PlaySound(CancellationToken token)
        {
            _audioSource.Play();

            try
            {
                await UniTask.WaitUntil(() => !_audioSource.isPlaying, cancellationToken: token);
            }
            catch
            {
                Debug.Log("SoundPlayer is already released.");
            }

            SoundManager.Instance.ReturnToPool(this);
        }

    }
}