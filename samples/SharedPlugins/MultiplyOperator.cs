using Shared;

namespace SharedPlugins
{
    public class MultiplyOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x * y;
        }
    }
}