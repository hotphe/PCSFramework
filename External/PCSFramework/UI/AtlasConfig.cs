using PCS.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using VInspector;

namespace PCS.UI
{
    [CreateAssetMenu(fileName = "AtalsConfig", menuName = "Config")]
    public class AtlasConfig : ScriptableObject
    {
        public SerializedDictionary<AtlasType, SpriteAtlas> AtlasDataDictionary = new SerializedDictionary<AtlasType, SpriteAtlas>();
    }

    public enum AtlasType
    {
        BackGround,
        Icon,
        Particle,
        Etc
    }
}