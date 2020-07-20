using Shared;

namespace WpfApp
{
    public class DivideOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x / y;
        }
    }
}
