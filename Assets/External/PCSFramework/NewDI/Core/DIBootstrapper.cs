using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using PCS.DI.Injector;

#if PCS_SceneManagement
using PCS.SceneManagement;
#endif

[assembly: AlwaysLinkAssembly]

namespace PCS.DI.Core
{
    internal static class DIBootstrapper
    {
        internal static Action<Scene, SceneScope> OnSceneLoaded;
        internal static Container ProjectContainer { get; private set; }
        internal static Dictionary<Scene, Container> ContainersPerScene { get; } = new();

#if PCS_SceneManagement
        internal static Dictionary<string, List<string>> SceneContainerHierarchy { get; } = new();
        internal static bool UseAutoContainerHierarch = false;
#endif
        internal static Dictionary<string, Container> SceneContainerParentOverride { get; } = new();
        internal static Dictionary<Scene, Action<ContainerBuilder>> ScenePreInstaller { get; } = new();

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Boot()
        {
            ContainersPerScene.Clear();
            SceneContainerParentOverride.Clear();
            ScenePreInstaller.Clear();
            ProjectContainer = CreateProjectContainer.Create();
            void InjectScene(Scene scene, SceneScope sceneScope)
            {
                var sceneContainer = CreateSceneContainer(scene, ProjectContainer, sceneScope);
                ContainersPerScene.Add(scene, sceneContainer);
                SceneInjector.Inject(scene, sceneContainer);
#if PCS_SceneManagement
                if (!SceneContainerHierarchy.TryGetValue(scene.name, out var additiveScenes))
                    return;
                foreach (var additive in additiveScenes)
                    SceneContainerParentOverride[additive] = sceneContainer;
#endif
            }

            void DisposeScene(Scene scene)
            {
                if (ContainersPerScene.Remove(scene, out var sceneContainer)) // Not all scenes has containers
                    sceneContainer.Dispose();
            }

            void DisposeProject()
            {
                ProjectContainer.Dispose();
                ProjectContainer = null;

                OnSceneLoaded -= InjectScene;
                UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= DisposeScene;
                Application.quitting -= DisposeProject;
            }

            OnSceneLoaded += InjectScene;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += DisposeScene;
            Application.quitting += DisposeProject;
            
#if PCS_SceneManagement
            SceneContainerHierarchy.Clear();
            SetSceneContainerHierarchy();
#endif
        }

        private static Container CreateSceneContainer(Scene scene, Container projectContainer, SceneScope sceneScope)
        {
            var sceneParentContainer = SceneContainerParentOverride.Remove(scene.name, out var container)
                ? container
                : projectContainer;
            
            return sceneParentContainer.Scope(builder =>
            {
                builder.SetName($"{scene.name} ({scene.GetHashCode()})");

                if (ScenePreInstaller.Remove(scene, out var preInstaller))
                {
                    preInstaller.Invoke(builder);
                }

                sceneScope.InstallBindings(builder);
            });
        }

#if PCS_SceneManagement
        private static void SetSceneContainerHierarchy()
        {
            var hierarchySO = Resources.Load<SceneConfig>(SceneConfig.FileName);

            if (hierarchySO == null)
                return;
            UseAutoContainerHierarch = hierarchySO.UseAutoContainerHierarchy;
            foreach (var sceneGroup in hierarchySO.Hierarchy)
            {
                SceneContainerHierarchy[sceneGroup.ActiveSceneName] = sceneGroup.AdditiveSceneNames;
            }
        }
#endif
    }
}
