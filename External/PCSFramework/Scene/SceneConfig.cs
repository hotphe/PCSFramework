using Eflatun.SceneReference;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.Scene
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Config/SceneConfig")]
    public class SceneConfig : ScriptableObject
    {
        [field: SerializeField] public List<SceneGroup> LoadSceneGroups { get; private set; } = new List<SceneGroup>();
        [field: SerializeField] public List<SceneReference> EssentialScene { get; private set; } = new List<SceneReference> ();
    }
}
