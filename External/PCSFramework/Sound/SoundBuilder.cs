using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.Sound
{
    public class SoundBuilder
    {
        private readonly SoundManager _soundManager;
        private Vector3 position = Vector3.zero;
        private bool randomPitch = false;

        public SoundBuilder(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public SoundBuilder WithPosition(Vector3 position)
        {
            this.position = position;
            return this;
        }

        public SoundBuilder WithRandomPitch()
        {
            randomPitch = true;
            return this;
        }

        public void Play(string name)
        {
            SoundData data = _soundManager.TryGetSoundData(name);

            if (data == null)
            {
                Debug.Log($"Data({name}) is null.");
                return;
            }

            if (!_soundManager.CanPlaySound(data))
                return;

            SoundPlayer soundPlayer = _soundManager.Get();
            soundPlayer.Initialize(data);
            soundPlayer.transform.position = position;
            soundPlayer.transform.parent = _soundManager.transform;

            if (randomPitch)
            {
                soundPlayer.WithRandomPitch();
            }

            if(data.FrequentSound)
            {
                soundPlayer.Node = _soundManager.FrequentSoundEmitters.AddLast(soundPlayer);
            }

            soundPlayer.Play();
        }

    }
}