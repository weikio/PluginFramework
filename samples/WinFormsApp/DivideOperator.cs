using Weikio.PluginFramework.Samples.Shared;

namespace WinFormsApp
{
    public class DivideOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x / y;
        }
    }
}
