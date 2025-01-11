using Cysharp.Threading.Tasks;

namespace PCS.SceneManagement
{
    public static class SceneLoader
    {
        public static void ToLobby()
        {
            //SceneManager.Instance.LoadSceneAsync<LobbyPresenter>(scene => scene.InitializeAsync(2)).Forget();
        }
        public static void ToMyScene2()
        {
            SceneManager.Instance.LoadSceneAsync<MyScene2Presenter>(scene => scene.InitializeAsync(2)).Forget();
        }
        public static void ToMyScene1()
        {
            SceneManager.Instance.LoadSceneAsync<MyScene1Presenter>(scene => scene.InitializeAsync(2)).Forget();
        }
    }
}
