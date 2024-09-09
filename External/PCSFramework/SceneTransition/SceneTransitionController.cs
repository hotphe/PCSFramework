using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace PCS.UI
{
    public static class SceneTransitionController
    {
        private static Subject<Unit> _onShow = new Subject<Unit>();
        private static Subject<Unit> _onHide = new Subject<Unit>();
        public static IObservable<Unit> OnShow => _onShow;
        public static IObservable<Unit> OnHide => _onHide;

        public static bool IsTransitioning;
        public static void Show()
        {
            IsTransitioning = true;
            _onShow.OnNext(Unit.Default);
        }

        public static void Hide()
        {
            IsTransitioning = false;
            _onHide.OnNext(Unit.Default);
        }

    }
}
