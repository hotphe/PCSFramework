using UnityEngine;

namespace PCS.Common
{
    public static class AndroidToastText
    {
        public enum Position
        {
            Default,
            Center,
            Bottom,
        }

        //https://www.ipentec.com/document/android-change-toast-position
        private const int PositionCenter = 0x00000011;
        private const int PositionBottom = 0x00000050;
        private const int PositionTop = 0x00000030;
        private const int PositionLeft = 0x00000003;
        private const int PositionRight = 0x00000005;

        public static void ShowToast(string text, Position position = Position.Default)
        {
            if (Application.platform != RuntimePlatform.Android) return;
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

                activity.Call("runOnUiThread", new AndroidJavaRunnable(
                    () =>
                    {
                        var Toast = new AndroidJavaClass("android.widget.Toast");
                        var javaString = new AndroidJavaObject("java.lang.String", text);
                        var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                        var toast = Toast.CallStatic<AndroidJavaObject>("makeText", context, javaString, Toast.GetStatic<int>("LENGTH_SHORT"));
                        switch (position)
                        {
                            case Position.Bottom:
                                toast.Call("setGravity", PositionBottom, 0, 0);
                                break;
                            case Position.Center:
                                toast.Call("setGravity", PositionCenter, 0, 0);
                                break;
                            default: break;
                        }

                        toast.Call("show");
                    }
                ));
            }
        }
    }
}
