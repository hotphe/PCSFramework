using PCS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PCS.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UIAdjuster))]
    public class UIAdjusterLocker : MonoBehaviour
    {
        [SerializeField] private bool isLock = true;
        private AspectRatioFitter _ratioFitterBackground;
        private UIAdjuster _uiAdjuster;
#if UNITY_EDITOR && !PRODUCTION
        private void OnValidate()
        {
            InitializeComponents();
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
                return;

            if (!isLock)
                return;

            _uiAdjuster.Apply(new Vector2Int(Screen.width, Screen.height));
        }
        private void InitializeComponents()
        {
            if (_uiAdjuster == null)
                _uiAdjuster = GetComponent<UIAdjuster>();
        }

    #endif
    }
}