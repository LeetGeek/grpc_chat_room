using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Jvh.Service;
using Jvh.Service.Chat;

namespace Jvh.App.ChatServer
{
    public class ChatServerImpl : ChatService.ChatServiceBase
    {
        static ChatRoomManager _chatRoomManager = new ChatRoomManager();

        private readonly object _lock = new object();
        private List<MessageClient> _messageClients = new List<MessageClient>();

        private ConcurrentQueue<ChatMessage> _messageQueue = new ConcurrentQueue<ChatMessage>();

        class MessageClient
        {

            public string Username { get; set; }
            public ServerCallContext context { get; set; }
            public IServerStreamWriter<ChatMessage> responseStream { get; set; }

            public MessageClient(string username, ServerCallContext context, IServerStreamWriter<ChatMessage> responseStream)
            {
                this.context = context;
                this.responseStream = responseStream;
                this.Username = username;
            }
        }


        public ChatServerImpl() : base()
        {
            BackgroundTask();
        }

        public override Task<ChatResponse> Login(UserInfo request, ServerCallContext context)
        {
             _chatRoomManager.JoinChat(request.Username);
             return Task.FromResult(new ChatResponse() { ErrorCode = 0, ErrorMessage = "", Name = ""});
        }

        public override Task<ChatResponse> Logout(UserInfo request, ServerCallContext context)
        {
            _chatRoomManager.LeaveChat(request.Username);
            return Task.FromResult(new ChatResponse() { ErrorCode = 0, ErrorMessage = "", Name = "" });
        }

        public override Task<ChatResponse> SendMessage(ChatMessage request, ServerCallContext context)
        {
            try
            {
                //_chatRoomManager.PublishChatMessage(request);
                _messageQueue.Enqueue(request);
                return Task.FromResult(new ChatResponse());
            }
            catch (RpcException rpcException)
            {
                Console.WriteLine(rpcException);
                return Task.FromResult(new ChatResponse());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(new ChatResponse());
            }
        }


        public override async Task ListenForMessageUpdates(UserInfo request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            AddMessageClient(request.Username, responseStream, context);

            //while (!context.CancellationToken.IsCancellationRequested && _chatRoomManager.IsUserInChatRoom(request.Username))
            //{
            //    var messages = _chatRoomManager.GetUnreadChatMessagesForUser(request.Username);
            //    foreach (var chatMessage in messages)
            //    {
            //        await responseStream.WriteAsync(chatMessage);
            //    }

            //    await Task.Delay(50);
            //}
        }

        public override async Task ListenForUserUpdates(UserInfo request, IServerStreamWriter<UserUpdate> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested && _chatRoomManager.IsUserInChatRoom(request.Username))
            {
                var userUpdates = _chatRoomManager.GetUnreadUserUpdatesForUser(request.Username);
                foreach (var userUpdate in userUpdates)
                {
                    await responseStream.WriteAsync(userUpdate);
                }

                await Task.Delay(50);
            }
        }

        public void StartPinging()
        {
            var testClient = new TestClient("localhost", 50052);
            Task.Run(() =>
            {
                while (true)
                {
                    var time = testClient.PingServer().Result;
                    Console.WriteLine($"Ping completed in {time.TotalMilliseconds/1000:F9} seconds");

                    Thread.Sleep(100);
                }
            });
        }

        private void AddMessageClient(string clientName, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context)
        {
            lock (_lock)
            {
                if (_messageClients.FirstOrDefault(o => o.Username == clientName) == null)
                {
                    var messageClient = new MessageClient(clientName, context, responseStream);
                    _messageClients.Add(messageClient);
                }
            }
        }
        private void RemoveMessageClient(string clientName)
        {
            lock (_lock)
            {
                _messageClients.RemoveAll(o => o.Username == clientName);
            }
        }

        private void RemoveMessageClient(MessageClient client)
        {
            lock (_lock)
            {
                _messageClients.Remove(client);
            }
        }

        private void BackgroundTask()
        {
            Task.Run( async () =>
            {
                IEnumerable<MessageClient> list;

                while (true)
                {
                    lock (_lock) list = _messageClients.ToList();

                    if (_messageQueue.TryDequeue(out var message))
                    {
                        foreach (var messageClient in list)
                        {
                            try
                            {
                                if (messageClient.context.CancellationToken.IsCancellationRequested)
                                {
                                    RemoveMessageClient(messageClient);
                                }
                                await messageClient.responseStream.WriteAsync(message);
                            }
                            catch (RpcException rpce)
                            {
                                Console.WriteLine(rpce);
                                RemoveMessageClient(messageClient);
                            }
                        }
                    }

                    await Task.Delay(5);
                }
            });
        }
    }

    class ChatRoomManager
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string, User> _users = new Dictionary<string, User>();
        private readonly List<ChatMessage> _chatMessages = new List<ChatMessage>();
        private readonly List<UserUpdate> _userUpdates = new List<UserUpdate>();

        

        public bool IsUserInChatRoom(string username)
        {
            lock (_lock)
                return _users.ContainsKey(username);
        } 


        public IEnumerable<string> GetUsers
        {
            get
            {
                lock (_lock) return _users.Keys.ToList();
            }
        }

        public void JoinChat(string username)
        {
            lock (_lock)
            {
                if (_users.ContainsKey(username))
                {
                    throw new Exception($"User already exists: {username}");
                }

                _users.Add(username, new User(username, _chatMessages.Count, _userUpdates.Count));
                _userUpdates.Add(new UserUpdate() { User = username, UserUpdateType = UserUpdateType.Login });
            }
        }

        public void LeaveChat(string username)
        {
            lock (_lock)
            {
                _users.Remove(username);
                _userUpdates.Add(new UserUpdate() { User = username, UserUpdateType = UserUpdateType.Logout });
            }
        }

        public IEnumerable<ChatMessage> GetUnreadChatMessagesForUser(string username)
        {
            lock (_lock)
            {
                if (_users[username].MessageIndex == _chatMessages.Count) return Enumerable.Empty<ChatMessage>();

                var index = _users[username].MessageIndex;
                _users[username].MessageIndex = _chatMessages.Count;

                lock(_lock)
                    return _chatMessages.Skip(index).ToList();
            }
        }

        public IEnumerable<UserUpdate> GetUnreadUserUpdatesForUser(string username)
        {
            lock (_lock)
            {
                var user = _users[username];
                var count = _userUpdates.Count;
                var index = user.UserUpdateIndex;

                if (index >= count) return Enumerable.Empty<UserUpdate>();

                var userUpdates = _userUpdates.Skip(index).ToList();
                _users[username].UserUpdateIndex = count;

                return userUpdates;
            }
        }

        public void PublishChatMessage(ChatMessage message)
        {
            lock (_lock)
            {
                _chatMessages.Add(message);
            }
        }
    }



    class User
    {
        public string Username { get; }
        public int MessageIndex { get; set; }
        public int UserUpdateIndex { get; set; }

        public User(string username)
        {
            Username = username;
            MessageIndex = 0;
            UserUpdateIndex = 0;
        }

        public User(string username, int messageIndex, int userUpdateIndex)
        {
            Username = username;
            MessageIndex = messageIndex;
            UserUpdateIndex = userUpdateIndex;
        }
    }
}