using Shared;

namespace SharedPlugins
{
    public class MinusOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x - y;
        }
    }
}