using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
using PCS.DI.Core;

namespace PCS.DI.Injector
{
    internal static class SceneInjector
    {
        internal static void Inject(Scene scene, Container container)
        {
            using var pooledObject1 = ListPool<GameObject>.Get(out var rootGameObjects);
            scene.GetRootGameObjects(rootGameObjects);
            GameObjectInjector.InjectRecursiveMany(rootGameObjects, container);
        }
    }
}