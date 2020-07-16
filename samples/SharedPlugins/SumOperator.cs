using Shared;

namespace SharedPlugins
{
    public class SumOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x + y;
        }
    }
}