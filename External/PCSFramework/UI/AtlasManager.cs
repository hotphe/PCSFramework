using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using PCS.UI;
using UnityEditorInternal;

namespace PCS.Common
{
    public class AtlasManager : MonoSingleton<AtlasManager>
    {
        private AtlasConfig _atlasConfig;

        //Sprite size used as a reference when using SpriteRenderer.
        public Vector2 DefaultSpriteSize { get; private set; }

        private const string DEFAULT_SPRITE = "Default_Sprite";

        public async UniTask InitializeAsync()
        {
            DontDestroyOnLoad(gameObject);
            _atlasConfig = await AddressableManager.LoadAssetAsync<AtlasConfig>(typeof(AtlasConfig).Name,false);
            DefaultSpriteSize = GetAtlas(AtlasType.Etc).GetSprite(DEFAULT_SPRITE).bounds.size;
        }

        public SpriteAtlas GetAtlas(AtlasType atlasType)
        {
            if (_atlasConfig == null)
                Debug.Log("널이야1");


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
