using System;

namespace PCS.SceneManagement
{
    public static class SceneTransitionController
    {
        public static event Action OnShow;
        public static event Action OnHide;

        public static bool IsTransitioning;
        public static void Show()
        {
            IsTransitioning = true;
            OnShow?.Invoke();
        }

        public static void Hide()
        {
            IsTransitioning = false;
            OnHide?.Invoke();
        }

    }
}
