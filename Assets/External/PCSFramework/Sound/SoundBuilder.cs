using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.Sound
{
    public class SoundBuilder
    {
        private readonly SoundManager _soundManager;
        private Vector3 _position = Vector3.zero;
        private bool _randomPitch = false;

        private string _currentSound = string.Empty;
        private SoundPlayer _currentSoundPlayer;

        public SoundBuilder(SoundManager soundManager)
        {
            _soundManager = soundManager;
        }

        public SoundBuilder WithPosition(Vector3 position)
        {
            this._position = position;
            return this;
        }

        public SoundBuilder WithRandomPitch()
        {
            _randomPitch = true;
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
            soundPlayer.transform.position = _position;
            soundPlayer.transform.parent = _soundManager.transform;

            if (_randomPitch)
            {
                soundPlayer.WithRandomPitch();
            }

            if(data.FrequentSound)
            {
                soundPlayer.Node = _soundManager.FrequentSoundEmitters.AddLast(soundPlayer);
            }

            soundPlayer.PlaySFX();
        }

        public void PlayBGM(string name)
        {
            if (_currentSound.Equals(name))
                return;

            _currentSound = name;
            SoundData data = _soundManager.TryGetSoundData(name);

            if (data == null)
            {
                Debug.Log($"Data({name}) is null.");
                return;
            }

            if (!_soundManager.CanPlaySound(data))
                return;
            if(_currentSoundPlayer == null)
                _currentSoundPlayer = _soundManager.Get();
            else
                _currentSoundPlayer.Stop();

            _currentSoundPlayer.Initialize(data);
            _currentSoundPlayer.transform.position = _position;
            _currentSoundPlayer.transform.parent = _soundManager.transform;

            _currentSoundPlayer.PlayBGM();
        }

    }
}