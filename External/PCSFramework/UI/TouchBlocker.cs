using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace PCS.UI
{
    public class TouchBlocker : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private void Awake()
        {
            TouchBlockerController.OnBlock.Subscribe(_ => Block()).AddTo(this);
            TouchBlockerController.OnRelease.Subscribe(_ => Release()).AddTo(this);

            _image.color = Color.magenta;
            Color tempColor = _image.color;
            tempColor.a = 0f;
            _image.color = tempColor;
        }

        private void Block() => _image.raycastTarget = true;
        private void Release() => _image.raycastTarget = false;
    }
}
