using System;

namespace Weikio.PluginFramework.Samples.Shared
{
    public interface IPlugin
    {
        void Run();
    }
    
    public interface IOutPlugin
    {
        string Get();
    }

    public interface IWidget
    {
        string Title { get; }
    }

    public interface IDialog
    {
        void Show();
    }
}
