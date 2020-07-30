using System.ComponentModel;
using Weikio.PluginFramework.Samples.Shared;

namespace Weikio.PluginFramework.Samples.SharedPlugins
{
    [DisplayName("The multiplier plugin")]
    public class MultiplyOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x * y;
        }
    }
}
