using System;
using PCS.DI.Core;

namespace PCS.DI.Resolver
{
    public interface IObjectResolver : IDisposable
    {
        Lifetime Lifetime { get; }
        object Resolve(Container container);
    }
}
