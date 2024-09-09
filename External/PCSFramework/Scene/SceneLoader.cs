using Cysharp.Threading.Tasks;

namespace PCS.Scene
{
    public static class SceneLoader
    {
        public static void ToLobby()
        {
            SceneManager.Instance.LoadSceneAsync<LobbyPresenter>(scene => scene.InitializeAsync(2)).Forget();
        }
    }
}
