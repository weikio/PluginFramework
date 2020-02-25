using System;

namespace TestIntefaces
{
    public interface ICommand
    {
    }

    public interface IJsonVersionResolver
    {
        string GetVersion();
    }
}
