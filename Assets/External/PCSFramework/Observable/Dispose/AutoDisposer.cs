using System;
using UnityEngine;
namespace PCS.Observable
{
    public class AutoDisposer : MonoBehaviour
    {
        private DisposableBag _disposable;

        public void SetDisposable(IDisposable disposable) => _disposable.Add(disposable);
        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}
