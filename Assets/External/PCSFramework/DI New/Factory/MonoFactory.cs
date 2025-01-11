using PCS.DI;
using PCS.DI.Injector;
using PCS.DI.Core;
using UnityEngine;

public sealed class MonoFactory<TValue> : IFactory<TValue> where TValue : MonoBehaviour
{
    [Inject] private Container _container;
    [Inject] private TValue _prefab;

    public TValue Create()
    {
        var gameObject = GameObject.Instantiate(_prefab);
        GameObjectInjector.InjectRecursive(gameObject.gameObject, _container);
        gameObject.gameObject.SetActive(true);
        return gameObject;
        
    }
}
