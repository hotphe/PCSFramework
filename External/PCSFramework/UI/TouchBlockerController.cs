using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCS.Common;
using UniRx;
using System;

namespace PCS.UI
{
    public static class TouchBlockerController
    {
        private static Subject<Unit> _onBlock = new Subject<Unit>();
        private static Subject<Unit> _onRelease = new Subject<Unit>();

        public static IObservable<Unit> OnBlock => _onBlock;
        public static IObservable<Unit> OnRelease => _onRelease;

        private static int blockCount;
        public static bool IsBlocking => blockCount > 0;

        public static void Block()
        {
            blockCount++;
            Check();
        }

        public static void Release()
        {
            blockCount--;
            Check();
        }

        private static void Check()
        {
            if (IsBlocking)
                _onBlock.OnNext(Unit.Default);
            else
                _onRelease.OnNext(Unit.Default);
        }
    }
}