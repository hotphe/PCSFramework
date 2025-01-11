using UnityEngine.SceneManagement;
using PCS.DI.Core;

namespace PCS.DI.Extensions
{
    public static class SceneExtensions
    {
        public static Container GetSceneContainer(this Scene scene)
        {
            return DIBootstrapper.ContainersPerScene[scene];
        }
    }
}