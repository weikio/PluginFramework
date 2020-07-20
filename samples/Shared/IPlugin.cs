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
}
