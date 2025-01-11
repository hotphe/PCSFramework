using UnityEngine;

namespace PCS.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(UIAdjuster))]
    public class UIAdjusterLocker : MonoBehaviour
    {
        [SerializeField] private bool isLock = true;
        private UIAdjuster _uiAdjuster;

#if UNITY_EDITOR 
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

            _uiAdjuster.Apply(new Vector2(Screen.width, Screen.height));
        }

        private void InitializeComponents()
        {
            if (_uiAdjuster == null)
                _uiAdjuster = GetComponent<UIAdjuster>();
        }
#endif
    }
}