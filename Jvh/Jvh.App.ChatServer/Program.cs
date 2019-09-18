using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Jvh.Service;
using Jvh.Service.Chat;
using Jvh.Service.Test;

namespace Jvh.App.ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 50052;

            var server = new Server();

            var chatServiceImp = new ChatServerImpl();
            var testService = TestService.BindService(new TestServiceImpl());
            var chatService = ChatService.BindService(chatServiceImp);
            server.Services.Add(testService);
            server.Services.Add(chatService);
            server.Ports.Add("localhost", Port, ServerCredentials.Insecure);
            server.Start();

            //Grpc.Core.Server server = new Grpc.Core.Server
            //{
            //    Services = { ChatService.BindService(new ChatServerImpl()) },
            //    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            //};
            //server.Start();

            chatServiceImp.StartPinging();


            Console.WriteLine("ChatService server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
