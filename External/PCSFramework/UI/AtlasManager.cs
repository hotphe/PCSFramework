using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using PCS.UI;

namespace PCS.Common
{
    public class AtlasManager : MonoSingleton<AtlasManager>
    {
        private AtlasConfig _atlasConfig;


        private  Dictionary<string, SpriteAtlas> atlas = new Dictionary<string, SpriteAtlas>();

        public static async UniTask InitializeAsync()
        {


             var result = await AddressableManager.LoadAssetsAsync<SpriteAtlas>("Atlas",false);

            foreach(var item in result) 
            { 
                Debug.Log(item.name);
            }
        }


    }
}
