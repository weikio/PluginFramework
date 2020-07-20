using System.ComponentModel;
using Shared;

namespace SharedPlugins
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
