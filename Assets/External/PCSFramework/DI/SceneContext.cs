using PCS.DI;

public class SceneContext : ContextBase
{
    protected override LifecycleScope _scope => LifecycleScope.Scene;

    protected override void Awake()
    {
        base.Awake();

    }
}
