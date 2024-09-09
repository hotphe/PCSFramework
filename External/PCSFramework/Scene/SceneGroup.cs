using System.Collections.Generic;
using Eflatun.SceneReference;

namespace PCS.Scene
{
    [System.Serializable]
    public class SceneGroup
    {
        public string GroupName;
        public SceneReference ActiveScene;
        public List<SceneReference> AdditiveScenes = new List<SceneReference>();
    }
}


