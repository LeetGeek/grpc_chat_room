using System.Threading.Tasks;
using Grpc.Core;
using Jvh.Service.Chat;

namespace Jvh.App.ChatServer
{
    public class ChatServerImpl : ChatService.ChatServiceBase
    {
        


        public override Task<ChatResponse> Login(ChatRegistrationRequest request, ServerCallContext context)
        {
            return base.Login(request, context);
        }

        public override Task<ChatResponse> Logout(ChatRegistrationRequest request, ServerCallContext context)
        {
            return base.Logout(request, context);
        }

        public override Task<ChatResponse> SendMessage(ChatMessage request, ServerCallContext context)
        {
            return base.SendMessage(request, context);
        }


        public override Task ListenForMessageUpdates(UserInfo request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            return base.ListenForMessageUpdates(request, responseStream, context);
        }

        public override Task ListenForUserUpdates(UserInfo request, IServerStreamWriter<UserUpdate> responseStream, ServerCallContext context)
        {
            return base.ListenForUserUpdates(request, responseStream, context);
        }
    }
}