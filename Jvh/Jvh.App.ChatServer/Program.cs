using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Jvh.Service.Chat;

namespace Jvh.App.ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 50052;

            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { ChatService.BindService(new ChatServerImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("ChatService server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
