using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PCS.UI
{
    public class UILetterbox : MonoBehaviour
    {
        [SerializeField] private AspectRatioFitter _aspectRatioFitter;
        private void Start()
        {
            _aspectRatioFitter.aspectRatio = ScreenResolutionController.ReferenceScreenSize.x / ScreenResolutionController.ReferenceScreenSize.y;
        }
    }
}
