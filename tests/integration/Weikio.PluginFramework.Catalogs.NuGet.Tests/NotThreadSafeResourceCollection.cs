using Xunit;

namespace PluginFramework.Catalogs.NuGet.Tests
{
    [CollectionDefinition(nameof(NotThreadSafeResourceCollection), DisableParallelization = true)]
    public class NotThreadSafeResourceCollection { }
}
