using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Weikio.PluginFramework.Tests
{
    public class TestClass
    {
        
    }

    public class Test
    {
        // Roslyn generated?
        public dynamic Configuration { get; set; }
        
        [Fact]
        public void MyTest()
        {
            var myFuncTest = new Func<string, int, string>((s, i) =>
            {
                var y = s + i;

                return y;
            });
            
            var myActionTest = new Action<string, int>((s, i) =>
            {
                var y = s + i;
            });

            var asyncTest = new Action<Task<string>, int>(async (Task<string> s, int i) =>
            {
                var d = await s;
                var y = d + i;
            });

            Create(myFuncTest);
            Create(myActionTest);
            Create(asyncTest);
        }

        public void Create(MulticastDelegate multicastDelegate)
        {
            var methodInfo = multicastDelegate.GetMethodInfo();

            var parameters = methodInfo.GetParameters();
            var returnType = methodInfo.ReturnType;
        }
    }
}
