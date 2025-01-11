namespace PCS.DI.Core
{
    public interface IInstaller
    {
        void InstallBindings(ContainerBuilder containerBuilder);
    }
}