using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Jvh.Chat.Client
{
    public class Client
    {
        private ChatService.ChatServiceClient _client;
        private string _username = string.Empty;

        public EventHandler<UserUpdate> OnUserUpdate;
        public EventHandler<ChatMessage> OnChatMessage;


        public void Connect()
        {
            var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            _client = new ChatService.ChatServiceClient(channel);
        }

        public void Disconnect()
        {
           
        }

        public void Login(string username)
        {
            var response = _client.LoginToChat(new ChatRegistrationRequest() {Name = username});
            if (response.ErrorCode != 0)
            {
                throw new Exception(response.Message);
            }

            _username = username;
            var userInfo = new UserInfo(){Username = _username};

            Task.Run(() =>
            {
                using (var call = _client.ListenForMessageUpdates(userInfo))
                {
                    var responseStream = call.ResponseStream;
                    while (responseStream.MoveNext().Result)
                    {
                        var message = responseStream.Current;
                        OnChatMessage.Invoke(this,message);
                    }
                }
            });

            Task.Run(() =>
            {
                using (var call = _client.ListenForUserUpdates(userInfo))
                {
                    var responseStream = call.ResponseStream;
                    while (responseStream.MoveNext().Result)
                    {
                        var item = responseStream.Current;
                        OnUserUpdate.Invoke(this, item);
                    }
                }
            });

        }

        public void Logoff()
        {
            
            _username = string.Empty;
        }

        public void SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            _client.SendMessageAsync(new ChatMessage()
            {
                From = _username,
                Message = message,
                Timestamp = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                To = ""
            });
        }
    }
}