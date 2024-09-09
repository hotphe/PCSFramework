using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.UI
{
    public class ScreenSizeChangeListener : MonoBehaviour
    {
        private void OnRectTransformDimensionsChange()
        {
            UniTask.Create(async () =>
            {
                await UniTask.WaitForEndOfFrame(this);
                ScreenResolutionController.CheckUpdateDeviceResolution();
            }).Forget();
        }
    }
}
