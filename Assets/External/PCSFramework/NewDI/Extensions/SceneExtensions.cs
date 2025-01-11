using UnityEngine.SceneManagement;
using PCS.DI.Core;

namespace PCS.DI.Extension
{
    public static class SceneExtensions
    {
        public static Container GetSceneContainer(this Scene scene)
        {
            DIBootstrapper.ContainersPerScene.TryGetValue(scene, out var container);
            return container;
        }
    }
}