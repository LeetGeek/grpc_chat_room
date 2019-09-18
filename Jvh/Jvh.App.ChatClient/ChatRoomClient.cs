using System;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Jvh.Service.Chat;

namespace Jvh.App.ChatClient
{
    public class ChatRoomClient
    {
        private readonly Subject<ChatMessage> _subjectChatMessage = new Subject<ChatMessage>();
        private readonly Subject<UserUpdate> _subjectUserUpdate = new Subject<UserUpdate>();

        private ChatService.ChatServiceClient _client;
        private string _username = string.Empty;

        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_username);

        public IObservable<ChatMessage> ChatMessageObservable => _subjectChatMessage;
        public IObservable<UserUpdate> UserUpdateObservable => _subjectUserUpdate;

        public void Connect()
        {
            var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            _client = new ChatService.ChatServiceClient(channel);
        }

        public void Disconnect()
        {
            // how to disconnect?
        }




        public void Login(string username)
        {
            var response = _client.Login(new UserInfo() { Username = username });
            if (response.ErrorCode != 0)
            {
                throw new Exception(response.ErrorMessage);
            }

            _username = username;
            var userInfo = new UserInfo() { Username = _username };

            Task.Run(() =>
            {
                //Thread.Sleep(1000);
                using (var call = _client.ListenForMessageUpdates(userInfo))
                {
                    var responseStream = call.ResponseStream;
                    while (responseStream.MoveNext().Result)
                    {
                        var message = responseStream.Current;
                        _subjectChatMessage.OnNext(message);
                    }
                }
            });

            Task.Run(() =>
            {
                //Thread.Sleep(1000);
                using (var call = _client.ListenForUserUpdates(userInfo))
                {
                    var responseStream = call.ResponseStream;
                    while (responseStream.MoveNext().Result)
                    {
                        var userUpdate = responseStream.Current;
                        _subjectUserUpdate.OnNext(userUpdate);
                    }
                }
            });

        }

        public void Logoff()
        {
            _client.Logout(new UserInfo() {Username = _username});
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