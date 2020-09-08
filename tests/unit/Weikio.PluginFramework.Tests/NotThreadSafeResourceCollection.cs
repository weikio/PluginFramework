using Xunit;

namespace Weikio.PluginFramework.Tests
{
    [CollectionDefinition(nameof(NotThreadSafeResourceCollection), DisableParallelization = true)]
    public class NotThreadSafeResourceCollection { }
}
