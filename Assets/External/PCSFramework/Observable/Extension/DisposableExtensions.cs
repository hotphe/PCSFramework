using System;
using System.Collections.Generic;
using UnityEngine;
namespace PCS.Observable
{
    public static class DisposableExtensions
    {
        public static T AddTo<T>(this T disposable, ref DisposableBag bag)
        where T : IDisposable
        {
            bag.Add(disposable);
            return disposable;
        }

        public static T AddTo<T>(this T disposable, ICollection<IDisposable> disposables)
            where T : IDisposable
        {
            disposables.Add(disposable);
            return disposable;
        }

        public static T AddTo<T>(this T disposable, GameObject gameObject) where T : IDisposable
        {
            if (gameObject == null)
            {
                disposable.Dispose();
                return disposable;
            }

            var disposer = gameObject.GetComponent<AutoDisposer>();
            if (disposer == null)
                disposer = gameObject.AddComponent<AutoDisposer>();

            disposer.SetDisposable(disposable);
            return disposable;
        }

        public static T AddTo<T>(this T disposable, MonoBehaviour component) where T : IDisposable
        {
            return AddTo(disposable, component.gameObject);
        }
    }
}