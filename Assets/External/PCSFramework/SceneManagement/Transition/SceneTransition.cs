using UnityEngine;
using UnityEngine.UI;

namespace PCS.SceneManagement
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private Image _transitionImage;

        private void Awake()
        {
            SceneTransitionController.OnShow += Show;
            SceneTransitionController.OnHide += Hide;
        }

        private void Show()
        {
            _transitionImage.gameObject.SetActive(true);
        }
        private void Hide()
        {
            _transitionImage.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            SceneTransitionController.OnShow -= Show;
            SceneTransitionController.OnHide -= Hide;
        }
    }
}
