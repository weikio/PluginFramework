namespace Shared
{
    public class RemainderOperator : IOperator
    {
        public int Calculate(int x, int y)
        {
            return x % y;
        }
    }
}
