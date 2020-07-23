// using System;
// using System.Linq;
// using System.Linq.Expressions;
// using System.Reflection;
// using System.Threading.Tasks;
// using Xunit;
// using Xunit.Abstractions;
//
// namespace Weikio.PluginFramework.Tests
// {
//     public class TestClass
//     {
//         
//     }
//
//     public class Test
//     {
//         private readonly ITestOutputHelper _testOutputHelper;
//
//         public Test(ITestOutputHelper testOutputHelper)
//         {
//             _testOutputHelper = testOutputHelper;
//         }
//
//         // Roslyn generated?
//         public dynamic Configuration { get; set; }
//         
//         [Fact]
//         public void MyTest()
//         {
//             var myFuncTest = new Func<string, int, string>((s, i) =>
//             {
//                 var y = s + i;
//
//                 return y;
//             });
//             
//             var myActionTest = new Action<string, int>((s, i) =>
//             {
//                 var y = s + i;
//             });
//
//             var simpleAction = new Action(() =>
//             {
//                 _testOutputHelper.WriteLine("Hello action");
//             });
//             
//             var simpleFunc = new Func<string>(() =>
//             {
//                 return "Hello func";
//             });
//             
//             var asyncTest = new Action<Task<string>, int>(async (Task<string> s, int i) =>
//             {
//                 var d = await s;
//                 var y = d + i;
//             });
//             
//             Create(myFuncTest);
//             Create(myActionTest);
//             Create(asyncTest);
//
//             var type = CreateType(simpleFunc);
//
//             var hmmh = Activator.CreateInstance(type);
//             
//
//         }
//
//         public void Create(MulticastDelegate multicastDelegate)
//         {
//             var methodInfo = multicastDelegate.GetMethodInfo();
//
//             var parameters = methodInfo.GetParameters();
//             var returnType = methodInfo.ReturnType;
//         }
//
//         public Type CreateType(MulticastDelegate multicastDelegate)
//         {
//             var methodInfo = multicastDelegate.GetMethodInfo();
//             var types = methodInfo.GetParameters().Select(p => p.ParameterType);
//             types = types.Concat(new[] { methodInfo.ReturnType });
//
//             if (methodInfo.ReturnType == typeof(void))
//             {
//                 var actionType = Expression.GetActionType(types.ToArray());
//             }
//             else
//             {
//                 var funcType = Expression.GetFuncType(types.ToArray());
//
//                 return funcType;
//             }
//
//             return null;
//         }
//     }
//     
//         class Program
//     {
//         static void Main(string[] args)
//         {
//             Console.WriteLine("Hello World!");
//             var myFuncTest = new Func<string, int, string>((s, i) =>
//             {
//                 var y = s + i;
//
//                 return y;
//             });
//             
//             var test = new Test();
//
//             var ass = test.CreateAssembly(myFuncTest);
//             var t = ass.GetExportedTypes().First();
//
//             dynamic obj = Activator.CreateInstance(t);
//
//             var res = obj.Run("Hello there", 25);
//             
//             Console.WriteLine(res);
//
//         }
//     }
//     //
//     // public class Test
//     // {
//     //     public static Dictionary<Guid, MulticastDelegate> Cache = new Dictionary<Guid, MulticastDelegate>();
//     //     
//     //     public Assembly CreateAssembly(MulticastDelegate multicastDelegate)
//     //     {
//     //         try
//     //         {
//     //             var id = Guid.NewGuid();
//     //             Cache.Add(id, multicastDelegate);
//     //             
//     //             var generator = new AssemblyGenerator();
//     //             generator.ReferenceAssemblyContainingType<Action>();
//     //             generator.ReferenceAssemblyContainingType<Test>();
//     //
//     //             // if (_options.AdditionalReferences?.Any() == true)
//     //             // {
//     //             //     foreach (var assembly in _options.AdditionalReferences)
//     //             //     {
//     //             //         generator.ReferenceAssembly(assembly);
//     //             //     }
//     //             // }
//     //
//     //             var methodInfo = multicastDelegate.GetMethodInfo();
//     //
//     //             var parameters = methodInfo.GetParameters();
//     //             var returnType = methodInfo.ReturnType;
//     //
//     //             foreach (var parameterInfo in parameters)
//     //             {
//     //                 generator.ReferenceAssembly(parameterInfo.ParameterType.Assembly);
//     //             }
//     //             
//     //             generator.ReferenceAssembly(returnType.Assembly);
//     //
//     //             string assemblySourceCode;
//     //
//     //             using (var sourceWriter = new SourceWriter())
//     //             {
//     //                 sourceWriter.UsingNamespace("System");
//     //                 sourceWriter.UsingNamespace("System.Diagnostics");
//     //                 sourceWriter.UsingNamespace("System.Threading.Tasks");
//     //                 sourceWriter.UsingNamespace("System.Text");
//     //                 sourceWriter.UsingNamespace("System.Collections");
//     //                 sourceWriter.UsingNamespace("System.Collections.Generic");
//     //                 sourceWriter.UsingNamespace("ConsoleApp31");
//     //                 foreach (var parameterInfo in parameters)
//     //                 {
//     //                     sourceWriter.UsingNamespace(parameterInfo.ParameterType.Namespace);
//     //                 }
//     //
//     //                 sourceWriter.UsingNamespace(returnType.Namespace);
//     //
//     //                 sourceWriter.Namespace("MyFuncTestNs");
//     //
//     //                     sourceWriter.StartClass($"MyFuncTestClass");
//     //
//     //                     sourceWriter.WriteLine($"public {returnType.FullName} Run ({string.Join(", ", parameters.Select(x => x.ParameterType.FullName + " " + x.ParameterType.Name.ToLowerInvariant() + "1"))}) {{");
//     //                     sourceWriter.WriteLine($"var deleg = Test.Cache[System.Guid.Parse(\"{id.ToString()}\")];");
//     //                     sourceWriter.WriteLine($"return ({returnType.FullName}) deleg.DynamicInvoke({string.Join(", ", parameters.Select(x => x.ParameterType.Name.ToLowerInvariant() + "1"))});");
//     //                     sourceWriter.WriteLine("}");
//     //                     sourceWriter.FinishBlock(); // Finish the class
//     //
//     //                 sourceWriter.FinishBlock(); // Finish the namespace
//     //                 
//     //                 assemblySourceCode = sourceWriter.Code();
//     //             }
//     //
//     //             var result = generator.Generate(assemblySourceCode);
//     //
//     //             return result;
//     //         }
//     //         catch (Exception e)
//     //         {
//     //             throw;
//     //         }
//     //     }
//     //
//     //     // Roslyn generated?
//     //     public dynamic Configuration { get; set; }
//     //
//     //     public void MyTest()
//     //     {
//     //         var myFuncTest = new Func<string, int, string>((s, i) =>
//     //         {
//     //             var y = s + i;
//     //
//     //             return y;
//     //         });
//     //
//     //         var myActionTest = new Action<string, int>((s, i) =>
//     //         {
//     //             var y = s + i;
//     //         });
//     //
//     //         var simpleAction = new Action(() => { Console.WriteLine("Hello action"); });
//     //
//     //         var simpleFunc = new Func<string>(() => { return "Hello func"; });
//     //
//     //         var asyncTest = new Action<Task<string>, int>(async (Task<string> s, int i) =>
//     //         {
//     //             var d = await s;
//     //             var y = d + i;
//     //         });
//     //
//     //         Create(myFuncTest);
//     //         Create(myActionTest);
//     //         Create(asyncTest);
//     //
//     //         var type = CreateType(simpleFunc);
//     //
//     //         var hmmh = Activator.CreateInstance(type);
//     //     }
//     //
//     //     public void Create(MulticastDelegate multicastDelegate)
//     //     {
//     //         var methodInfo = multicastDelegate.GetMethodInfo();
//     //
//     //         var parameters = methodInfo.GetParameters();
//     //         var returnType = methodInfo.ReturnType;
//     //     }
//     //
//     //     public Type CreateType(MulticastDelegate multicastDelegate)
//     //     {
//     //         var methodInfo = multicastDelegate.GetMethodInfo();
//     //         var types = methodInfo.GetParameters().Select(p => p.ParameterType);
//     //         types = types.Concat(new[] {methodInfo.ReturnType});
//     //
//     //         if (methodInfo.ReturnType == typeof(void))
//     //         {
//     //             var actionType = Expression.GetActionType(types.ToArray());
//     //         }
//     //         else
//     //         {
//     //             var funcType = Expression.GetFuncType(types.ToArray());
//     //
//     //             return funcType;
//     //         }
//     //
//     //         return null;
//     //     }
//     // }
// }
