using Grpc.Core;
using LocationSerivce.BenchmarkProto;

namespace LocationService.Benchmark;

class SendRequest : BenchmarkService.BenchmarkServiceBase, IReflectionInstance
{

    public override Task<Response> HelloWorld(Hello request, ServerCallContext context)
    {
        return Task.FromResult(new Response() { Name = request.Name });
    }

}