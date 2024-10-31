using Cysharp.Threading.Tasks;

namespace PCS.Scene
{
    public static class SceneLoader
    {
        public static void ToTitle()
        {
            SceneManager.Instance.LoadSceneAsync<TitlePresenter>(scene=> scene.InitializeAsync()).Forget();
        }
        public static void ToMain()
        {
            SceneManager.Instance.LoadSceneAsync<MainPresenter>(scene => scene.InitializeAsync()).Forget();
        }
        public static void ToLobby()
        {
            //SceneManager.Instance.LoadSceneAsync<LobbyPresenter>(scene => scene.InitializeAsync(2)).Forget();
        }
    }
}
