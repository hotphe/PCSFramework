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
    
        [SerializeField] private RectTransform _ratioFitterBackgroundRect;
        [SerializeField] private bool isLock = true;
        [SerializeField] private bool isDebug = true;
        [SerializeField] private List<GameObject> _debugObjects = new List<GameObject>();

        private Vector2 _referenceScreenSize;
        private AspectRatioFitter _ratioFitterBackground;
        private AspectRatioFitter _ratioFitterInside;
        private UIAdjuster _uiAdjuster;
        private void Awake()
        {
#if PRODUCTION
            foreach(var v in _debugObjects)
                v.SetActive(false);
#endif
        }
#if UNITY_EDITOR && !PRODUCTION
        private void OnValidate()
        {
            InitializeComponents();
        }

        private void LateUpdate()
        {
            if (!isLock)
                return;

            if (isDebug)
            {
                foreach (var v in _debugObjects)
                    v.SetActive(true);
            }else
            {

                foreach (var v in _debugObjects)
                    v.SetActive(false);
            }

            _uiAdjuster.Apply(new Vector2Int(Screen.width, Screen.height));
        }
        private void InitializeComponents()
        {
            if (_uiAdjuster == null)
                _uiAdjuster = GetComponent<UIAdjuster>();
            if (_ratioFitterBackground == null && _ratioFitterBackgroundRect != null)
                _ratioFitterBackground = _ratioFitterBackgroundRect.GetComponent<AspectRatioFitter>();
        }

        private bool IsLongWidth()
        {
            return _referenceScreenSize.x / _referenceScreenSize.y < Screen.width / (float)Screen.height;
        }
    #endif
    }
}