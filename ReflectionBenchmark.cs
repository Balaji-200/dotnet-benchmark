using System;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Testing;
using LocationSerivce.BenchmarkProto;

namespace LocationService.Benchmark;

public class ReflectionBenchmark
{
    private IReflectionInstance instance = new SendRequest();
    private static readonly string functionName = "HelloWorld";
    private static Hello helloRequest = new() { Name = "Benchmark" };
    private static ByteString requestPayload = Serializer.Serialize(helloRequest);

    private readonly string csvContent = @"
            Name,Age,Country
John,25,USA
Alice,30,Canada
Bob,22,UK
Emily,28,Australia
Daniel,35,Germany
Sophia,26,France
Liam,24,USA
Olivia,31,Canada
Noah,27,UK
Ava,29,Australia
Mason,33,Germany
Isabella,23,France
Ethan,32,USA
Mia,21,Canada
Logan,34,UK
Charlotte,28,Australia
James,26,Germany
Amelia,30,France
Benjamin,25,USA
Harper,27,Canada
";

    private MethodInfo _m;
    private MethodInfo _serializer;
    private MethodInvoker _mInvoker;
    private MethodInvoker _serializerInvoker;
    private Func<object, object[], object> _delMethod;
    private Func<ByteString, IMessage> _delSerializer;

    [GlobalSetup]
    public void Setup()
    {
        _m = instance.GetType().GetMethod(functionName);
        Type typeOfInput = _m.GetParameters()[0].ParameterType;
        _serializer = typeof(Serializer).GetMethod("Deserialize").MakeGenericMethod(typeOfInput);
        _mInvoker = MethodInvoker.Create(_m);
        _serializerInvoker = MethodInvoker.Create(_serializer);
        _delSerializer = (Func<ByteString, IMessage>)Delegate.CreateDelegate(typeof(Func<ByteString, IMessage>), null, _serializer);
        _delMethod = CreateMethodDelegate(_m);
    }

    [Benchmark(Description = "MethodInfo.Invoke")]
    public void BenchmarkMethodInvoke()
    {
        var context = TestServerCallContext.Create(
        method: "MyMethod",
        host: "localhost",
        deadline: DateTime.UtcNow.AddMinutes(1),
        requestHeaders: new Metadata(),
        cancellationToken: CancellationToken.None,
        peer: "127.0.0.1",
        authContext: null,
        contextPropagationToken: null,
        writeHeadersFunc: (metadata) => Task.CompletedTask,
        writeOptionsGetter: default,
        writeOptionsSetter: default
        );
        var res = ExecuteMethod(context);
    }

    [Benchmark(Description = "MethodInvoker.Invoke")]
    public void BenchmarkMethodInvoker()
    {
        var context = TestServerCallContext.Create(
        method: "MyMethod",
        host: "localhost",
        deadline: DateTime.UtcNow.AddMinutes(1),
        requestHeaders: new Metadata(),
        cancellationToken: CancellationToken.None,
        peer: "127.0.0.1",
        authContext: null,
        contextPropagationToken: null,
        writeHeadersFunc: (metadata) => Task.CompletedTask,
        writeOptionsGetter: default,
        writeOptionsSetter: default
        );
        var res = ExecuteMethodInvoker(context);
    }

    [Benchmark(Description = "DelegateCall")]
    public void ExecuteMethodDelegate()
    {
        var context = TestServerCallContext.Create(
        method: "MyMethod",
        host: "localhost",
        deadline: DateTime.UtcNow.AddMinutes(1),
        requestHeaders: new Metadata(),
        cancellationToken: CancellationToken.None,
        peer: "127.0.0.1",
        authContext: null,
        contextPropagationToken: null,
        writeHeadersFunc: (metadata) => Task.CompletedTask,
        writeOptionsGetter: default,
        writeOptionsSetter: default
        );
        var res = ExecuteMethodDelegate(context);
    }

    [Benchmark(Description = "String Manipulation")]
    public void GetCSV()
    {
        GetLastRowFromCSV(csvContent);
    }

    private ByteString ExecuteMethod(ServerCallContext context)
    {
        IMessage message = new Hello { Name = "Exec" };
        if (_m != null && _m.GetParameters().Length == 2)
        {
            var res = _serializer
                .Invoke(null, new object[] { requestPayload });
            var result = _m.Invoke(instance, new object[] { res, context });
            message = (IMessage)result.GetType().GetProperty("Result")?.GetValue(result);
        }
        return Serializer.Serialize(message);
    }

    private ByteString ExecuteMethodInvoker(ServerCallContext context)
    {
        IMessage message = new Hello { Name = "Exec" };
        if (_m != null && _m.GetParameters().Length == 2)
        {
            var res = _serializerInvoker.Invoke(null, requestPayload);
            var result = _mInvoker.Invoke(instance, res, context);
            message = (IMessage)result.GetType().GetProperty("Result")?.GetValue(result);
        }
        return Serializer.Serialize(message);
    }

    private ByteString ExecuteMethodDelegate(ServerCallContext context)
    {
        IMessage message = new Hello { Name = "Exec" };
        if (_m != null && _m.GetParameters().Length == 2)
        {
            var res = _delSerializer(requestPayload);
            var result = _delMethod(instance, new object[] { res, context });
            message = (IMessage)result.GetType().GetProperty("Result")?.GetValue(result);
        }
        return Serializer.Serialize(message);
    }

    private static Func<object, object[], object> CreateMethodDelegate(MethodInfo methodInfo)
    {
        var instanceParam = Expression.Parameter(typeof(object), "instance");
        var argsParam = Expression.Parameter(typeof(object[]), "args");

        var paramExprs = methodInfo.GetParameters().Select((param, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                param.ParameterType
            )
        ).ToArray();

        var instanceCast = Expression.Convert(instanceParam, methodInfo.DeclaringType);
        var callExpr = Expression.Call(instanceCast, methodInfo, paramExprs);

        // If method returns void, return null
        Expression body = methodInfo.ReturnType == typeof(void)
            ? (Expression)Expression.Block(callExpr, Expression.Constant(null))
            : Expression.Convert(callExpr, typeof(object));

        return Expression.Lambda<Func<object, object[], object>>(body, instanceParam, argsParam).Compile();
    }

    private string GetLastRowFromCSV(string csvContent)
    {
        if (string.IsNullOrEmpty(csvContent))
        {
            return string.Empty;
        }

        // Split the CSV content into rows
        string[] rows = csvContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

        if (rows.Length == 0)
        {
            throw new ArgumentException("CSV content is empty");
        }

        // Get the last row
        string lastRow = rows.Last();

        return lastRow;
    }

}
