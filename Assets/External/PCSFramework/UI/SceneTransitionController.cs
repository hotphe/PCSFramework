using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCS.UI
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
