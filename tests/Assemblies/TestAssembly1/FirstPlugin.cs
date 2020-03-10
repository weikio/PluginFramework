using TestIntefaces;

namespace TestAssembly1
{
    public class FirstPlugin : ICommand
    {
        public string RunMe()
        {
            return "Hello from RunMe";
        }
    }
}
