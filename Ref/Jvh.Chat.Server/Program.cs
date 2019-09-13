using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Jvh.Chat.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 50052;

            Grpc.Core.Server server = new Grpc.Core.Server
            {
                Services = { ChatService.BindService(new ChatServiceImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("ChatService server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }

    class ChatServiceImpl : ChatService.ChatServiceBase
    {
        private static ChatUserManager _chatUserManager = new ChatUserManager();


        public ChatServiceImpl()
        {
            
        }

        public override Task<ChatRegistrationResponse> LoginToChat(ChatRegistrationRequest request, ServerCallContext context)
        {
            try
            {
                _chatUserManager.UserLogin(request.Name);
                return Task.FromResult(new ChatRegistrationResponse(){Name = request.Name});
            }
            catch (Exception e)
            {
                return Task.FromResult(new ChatRegistrationResponse() {ErrorCode = -1, Message = e.Message, Name = request.Name});
            }
        }

        public override async Task ListenForUserUpdates(UserInfo request, IServerStreamWriter<UserUpdate> responseStream, ServerCallContext context)
        {
            try
            {
                while (!context.CancellationToken.IsCancellationRequested && _chatUserManager.IsUserValid(request.Username))
                {
                    var userUpdates = _chatUserManager.GetUserUpdates(request.Username);

                    foreach (var userUpdate in userUpdates)
                    {
                        await responseStream.WriteAsync(userUpdate);
                    }

                    await Task.Delay(50);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override async Task ListenForMessageUpdates(UserInfo request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            try
            {
                while (!context.CancellationToken.IsCancellationRequested && _chatUserManager.IsUserValid(request.Username))
                {
                    var chatMessages = _chatUserManager.GetChatMessages(request.Username);

                    foreach (var chatMessage in chatMessages)
                    {
                        await responseStream.WriteAsync(chatMessage);
                    }

                    await Task.Delay(50);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override Task<ChatResponse> SendMessage(ChatMessage request, ServerCallContext context)
        {
            try
            {
                _chatUserManager.SendChatMessage(request);
                return Task.FromResult(new ChatResponse());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(new ChatResponse(){ErrorCode = -1, Message = e.Message, Name = request.From});
            }
            
        }
    }
}
