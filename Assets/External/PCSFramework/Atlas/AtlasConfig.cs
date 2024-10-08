using PCS.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace PCS.UI
{
    [CreateAssetMenu(fileName = "AtlasConfig", menuName = "Config/AtlasConfig")]
    public class AtlasConfig : ScriptableObject
    {
        public SerializedDictionary<AtlasType, SpriteAtlas> AtlasDataDictionary = new SerializedDictionary<AtlasType, SpriteAtlas>();
    }

    public enum AtlasType
    {
        Background,
        Icon,
        Character,
        Particle,
        Etc
    }
}