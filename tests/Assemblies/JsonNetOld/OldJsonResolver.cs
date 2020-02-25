using System;
using Newtonsoft.Json;
using TestIntefaces;

namespace JsonNetOld
{
    public class OldJsonResolver : IJsonVersionResolver
    {
        public string GetVersion()
        {
            var result = typeof(JsonConvert).Assembly.GetName().Version.ToString();

            return result;
        }
    }
}
