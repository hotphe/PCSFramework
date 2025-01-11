using Eflatun.SceneReference;
using System.Collections.Generic;
using UnityEngine;
using PCS.Common;
using System.Linq;

namespace PCS.SceneManagement
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "PCS/Scene/Config", order = 0)]
    public class SceneConfig : ScriptableObject
    {
        public const string FileName = "SceneConfig";
        public string StartSceneName;
#if PCS_DI
        [Tooltip("If you use PCS.DI framework, ")]
        public bool UseAutoContainerHierarchy;
#endif
        public bool UseLoadingScene;
        [Condition(nameof(UseLoadingScene), true)]
        public SceneReference LoadingScene;

        public List<SceneReference> EssentialScenes;
        public List<SceneGroup> Hierarchy = new List<SceneGroup>();

        private void OnValidate()
        {
            CheckCircularReference();
        }


        private void CheckCircularReference()
        {
            //추후 editor스크립트로 변경
        }

    }

    
    [System.Serializable]
    public struct SceneGroup
    {
        public SceneReference ActiveScene;
        public List<SceneReference> AdditiveScenes;
        public readonly string ActiveSceneName => ActiveScene.Name;
        public readonly List<string> AdditiveSceneNames => AdditiveScenes.Select(x=>x.Name).ToList();
    }
    
}
