using PCS.DI;
using PCS.DI.Injector;
using PCS.DI.Extension;
using UnityEngine;

public sealed class MonoFactory<TValue> : IFactory<TValue> where TValue : MonoBehaviour
{
    [Inject] private TValue _prefab;

    public TValue Create()
    {
        var value = GameObject.Instantiate(_prefab);
        GameObjectInjector.InjectRecursive(value.gameObject, value.gameObject.scene.GetSceneContainer());
        value.gameObject.SetActive(true);

        return value;
    }
}
