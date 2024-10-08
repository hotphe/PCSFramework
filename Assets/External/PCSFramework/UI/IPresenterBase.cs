using Cysharp.Threading.Tasks;

public interface IPresenterBase
{
    UniTask InitializeAsync(int index=0);
}
