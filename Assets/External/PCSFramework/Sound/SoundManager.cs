using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;
using PCS.Common;
using PCS.SaveData;
#if PCS_Addressable
using PCS.Addressable;
#endif

namespace PCS.Sound
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        public readonly LinkedList<SoundPlayer> FrequentSoundEmitters = new();
        private SoundConfig _soundConfig;

        private IObjectPool<SoundPlayer> _soundPlayerPool;
        private readonly List<SoundPlayer> _activeSoundPlayers = new List<SoundPlayer>();
        private SoundBuilder _bgmBuilder;

        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);
            _bgmBuilder = new SoundBuilder(this);
#if PCS_Addressable
            _soundConfig = await AddressableManager.LoadAssetAsync<SoundConfig>(typeof(SoundConfig).Name, false);
#else
            _soundConfig = (SoundConfig)await Resources.LoadAsync<SoundConfig>(typeof(SoundConfig).Name);
#endif
            if (_soundConfig == null )
            {
                Debug.LogError($"There is no {typeof(SoundConfig).Name} asset in addressables.");
                return;
            }    

            _soundPlayerPool = new ObjectPool<SoundPlayer>(
                CreateSoundPlayer,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                _soundConfig.CollectionCheck,
                _soundConfig.DefaultCapacity,
                _soundConfig.MaxPoolSize);

            SetMasterVolume(OptionSaveData.Instance.MasterVolume);
            SetBGMVolume(OptionSaveData.Instance.BGMVolume);
            SetSFXVolume(OptionSaveData.Instance.SFXVolume);
            
        }
        private float ConvertToDecibel(float value)
        {
            if (value <= 0) return -80f;
            return Mathf.Log10(value) * 20f;
        }

        public void SetMasterVolume(float volume)
        {
            _soundConfig.BaseAudioMixer.SetFloat("Master", ConvertToDecibel(volume));
        }

        // BGM 볼륨 조절
        public void SetBGMVolume(float volume)
        {
            _soundConfig.BaseAudioMixer.SetFloat("BGM", ConvertToDecibel(volume));
        }

        // SFX 볼륨 조절
        public void SetSFXVolume(float volume)
        {
            _soundConfig.BaseAudioMixer.SetFloat("SFX", ConvertToDecibel(volume));
        }

        public bool CanPlaySound(SoundData data)
        {
            if (!data.FrequentSound) return true;

            if (FrequentSoundEmitters.Count >= _soundConfig.MaxSoundInstances)
            {
                try
                {
                    FrequentSoundEmitters.First.Value.Stop();
                    return true;
                }
                catch
                {
                    Debug.Log("SoundPlayer is already released.");
                }
                return false;
            }
            return true;
        }

        public SoundData TryGetSoundData(string name)
        {
            if(_soundConfig.SoundDataDictionary.TryGetValue(name, out var soundData))
                return soundData;
            Debug.Log($"{name} dose not exist.");
            return null;
        }

        public SoundBuilder CreateSoundBuilder() => new SoundBuilder(this);

        public SoundPlayer Get()
        {
            return _soundPlayerPool.Get();
        }

        public SoundBuilder GetBGMBuilder() => _bgmBuilder;

        public void ReturnToPool(SoundPlayer player)
        {
            _soundPlayerPool.Release(player);
        }


        private SoundPlayer CreateSoundPlayer()
        {
            var soundPlayer = Instantiate(_soundConfig.SoundPlayerPrefab);
            if(soundPlayer == null)
            {
                Debug.LogError("SoundPlayerPrefab is not set in the SoundConfig. Create Object and add Component is not recommanded. For performance, set SoundPlayerPrefab.");
                soundPlayer = new GameObject("SoundPlayer_AutoGenerated", typeof(SoundPlayer),typeof(AudioSource)).GetComponent<SoundPlayer>();
            }
            soundPlayer.gameObject.SetActive(false);
            return soundPlayer;
        }

        private void OnTakeFromPool(SoundPlayer player)
        {
            player.gameObject.SetActive(true);
            _activeSoundPlayers.Add(player);
        }

        private void OnReturnedToPool(SoundPlayer player)
        {
            player.gameObject.SetActive(false);
            _activeSoundPlayers.Remove(player);
            }

        private void OnDestroyPoolObject(SoundPlayer player)
        {
            Destroy(player.gameObject);
        }



    }
}