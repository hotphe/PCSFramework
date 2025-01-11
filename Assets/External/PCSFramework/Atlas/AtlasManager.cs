using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using PCS.UI;
#if PCS_Addressable
using PCS.Addressable;
#endif

namespace PCS.Common
{
    public class AtlasManager : MonoSingleton<AtlasManager>
    {
        private AtlasConfig _atlasConfig;

        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);
#if PCS_Addressable
            _atlasConfig = await AddressableManager.LoadAssetAsync<AtlasConfig>(typeof(AtlasConfig).Name,false);
#else
            _atlasConfig = (AtlasConfig) await Resources.LoadAsync<AtlasConfig>(typeof(AtlasConfig).Name);
#endif

        }

        public SpriteAtlas GetAtlas(AtlasType atlasType)
        {
            if (_atlasConfig == null)
                Debug.Log("AtalsConfig is null.");


            if(_atlasConfig.AtlasDataDictionary.TryGetValue(atlasType, out var spriteAtlas)) 
            {
                return spriteAtlas;
            }
            Debug.LogError($"There is no Atlas type of {atlasType}");
            return null;
        }

        public Sprite GetSprite(AtlasType atlasType, string spriteName)
        {
            if(_atlasConfig.AtlasDataDictionary.TryGetValue(atlasType, out var spriteAtlas))
            {
                return spriteAtlas.GetSprite(spriteName);
            }
            Debug.LogError($"There is no Atlas type of {atlasType}");
            return null;
        }
    }
}
