using UnityEngine;
using UnityEngine.UI;

namespace PCS.UI
{
    public class TouchBlocker : MonoBehaviour
    {
        [SerializeField] private Image _image;

        private void Awake()
        {
            TouchBlockerController.OnBlock += Block;
            TouchBlockerController.OnRelease += Release;

            _image.color = Color.magenta;
            Color tempColor = _image.color;
            tempColor.a = 0f;
            _image.color = tempColor;
        }

        private void Block() => _image.raycastTarget = true;
        private void Release() => _image.raycastTarget = false;

        private void OnDestroy()
        {
            TouchBlockerController.OnBlock -= Block;
            TouchBlockerController.OnRelease -= Release;
        }
    }
}
