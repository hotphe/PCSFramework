using Cysharp.Threading.Tasks;

namespace PCS.Common
{
    public interface IPresenterBase
    {
        UniTask InitializeAsync(int index = 0);
    }
}