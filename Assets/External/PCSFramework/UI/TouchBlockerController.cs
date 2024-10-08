using System;

namespace PCS.UI
{
    public static class TouchBlockerController
    {
        public static event Action OnBlock;
        public static event Action OnRelease;

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
                OnBlock?.Invoke();
            else
                OnRelease?.Invoke();
        }
    }
}