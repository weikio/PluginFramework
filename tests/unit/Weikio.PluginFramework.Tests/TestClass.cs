using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Weikio.PluginFramework.Tests
{
    public class TestClass
    {
        
    }

    public class Test
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public Test(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

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

            var simpleAction = new Action(() =>
            {
                _testOutputHelper.WriteLine("Hello action");
            });
            
            var simpleFunc = new Func<string>(() =>
            {
                return "Hello func";
            });
            
            var asyncTest = new Action<Task<string>, int>(async (Task<string> s, int i) =>
            {
                var d = await s;
                var y = d + i;
            });
            
            Create(myFuncTest);
            Create(myActionTest);
            Create(asyncTest);

            var type = CreateType(simpleFunc);

            var hmmh = Activator.CreateInstance(type);
            

        }

        public void Create(MulticastDelegate multicastDelegate)
        {
            var methodInfo = multicastDelegate.GetMethodInfo();

            var parameters = methodInfo.GetParameters();
            var returnType = methodInfo.ReturnType;
        }

        public Type CreateType(MulticastDelegate multicastDelegate)
        {
            var methodInfo = multicastDelegate.GetMethodInfo();
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);
            types = types.Concat(new[] { methodInfo.ReturnType });

            if (methodInfo.ReturnType == typeof(void))
            {
                var actionType = Expression.GetActionType(types.ToArray());
            }
            else
            {
                var funcType = Expression.GetFuncType(types.ToArray());

                return funcType;
            }

            return null;
        }
    }
}
