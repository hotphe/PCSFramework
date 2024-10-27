using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using PCS.UI;

namespace PCS.Common
{
    public class AtlasManager : MonoSingleton<AtlasManager>
    {
        private AtlasConfig _atlasConfig;

        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);
            _atlasConfig = await AddressableManager.LoadAssetAsync<AtlasConfig>(typeof(AtlasConfig).Name,false);
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
