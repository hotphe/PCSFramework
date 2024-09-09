using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

namespace PCS.UI
{
    public class SceneTransition : MonoBehaviour
    {
        [SerializeField] private Image _transitionImage;

        private void Awake()
        {
            SceneTransitionController.OnShow.Subscribe(_ => Show()).AddTo(this);
            SceneTransitionController.OnHide.Subscribe(_ => Hide()).AddTo(this);
        }

        private void Show()
        {
            _transitionImage.gameObject.SetActive(true);
        }
        private void Hide()
        {
            _transitionImage.gameObject.SetActive(false);
        }
    }
}
