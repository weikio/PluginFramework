using Newtonsoft.Json;
using TestIntefaces;

namespace JsonNetNew
{
    public class NewJsonResolver : IJsonVersionResolver
    {
        public string GetVersion()
        {
            var result = typeof(JsonConvert).Assembly.GetName().Version.ToString();

            return result;
        }
    }
}
