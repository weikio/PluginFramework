using System;

namespace Weikio.PluginFramework.Catalogs.Roslyn
{
    public class InvalidCodeException : Exception
    {
        public InvalidCodeException(Exception exception) : this("", exception)
        {
        }

        public InvalidCodeException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}
