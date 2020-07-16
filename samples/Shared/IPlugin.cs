using System;

namespace Shared
{
    public interface IPlugin
    {
        void Run();
    }
    
    public interface IOutPlugin
    {
        string Get();
    }

    public interface IOperator
    {
        int Calculate(int x, int y);
    }
}
