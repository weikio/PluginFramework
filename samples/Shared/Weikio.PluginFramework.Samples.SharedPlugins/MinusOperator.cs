using Weikio.PluginFramework.Samples.Shared;

namespace Weikio.PluginFramework.Samples.SharedPlugins
{
    public class MinusOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x - y;
        }
    }
}