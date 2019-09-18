using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Jvh.Service.Test;

namespace Jvh.Service
{
    public class TestServiceImpl : TestService.TestServiceBase
    {
        public override async Task<PingResponse> Ping(PingRequest request, ServerCallContext context)
        {
            await Task.Delay(1000);
            return new PingResponse() {Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)};
        }

        public static Grpc.Core.Server LaunchService(string host, int port)
        {
            var server = new Server();
            var serviceDef = TestService.BindService(new TestServiceImpl());
            server.Services.Add(serviceDef);
            server.Ports.Add(host, port, ServerCredentials.Insecure);
            server.Start();
            return server;
        }
    }

    public class TestClient
    {
        private TestService.TestServiceClient _client;

        public TestClient(string host, int port)
        {
            var channel = new Channel(host,port,ChannelCredentials.Insecure);
            _client = new TestService.TestServiceClient(channel);
        }

        public async Task<TimeSpan> PingServer()
        {
            var request = new PingRequest();
            request.Timestamp = Timestamp.FromDateTime(DateTime.UtcNow);
            var response = await _client.PingAsync(request);

            var duration = response.Timestamp - request.Timestamp;
            return duration.ToTimeSpan();
        }
    }
}
