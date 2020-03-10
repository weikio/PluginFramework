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
        
        public string GetLoggingVersion()
        {
            var logging = new Microsoft.Extensions.Logging.LoggerFactory();
            Console.WriteLine(logging.ToString());
            
            var result = typeof(Microsoft.Extensions.Logging.LoggerFactory).Assembly.GetName().Version.ToString();

            return result;
        }
    }
}
