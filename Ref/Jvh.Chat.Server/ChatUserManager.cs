using System;
using System.Collections.Generic;
using System.Linq;

namespace Jvh.Chat.Server
{
    class User
    {
        public string Username { get; }
        public int UserUpdateIndex { get; set; }
        public int ChatMessageIndex { get; set; }

        public User(string username)
        {
            Username = username;
            UserUpdateIndex = 0;
            ChatMessageIndex = 0;
        }
    }
    class ChatUserManager
    {
        private static ChatUserManager _Instance;
        public static ChatUserManager Instance
        {
            get { return _Instance ?? (_Instance = new ChatUserManager()); }
        }


        private readonly object _lock = new object();
        private readonly List<ChatMessage> _chatMessages = new List<ChatMessage>();
        private readonly List<UserUpdate> _userUpdates = new List<UserUpdate>();
        private readonly Dictionary<string,User> _users = new Dictionary<string, User>();

        public void UserLogin(string username)
        {
            lock (_lock)
            {
                if(_users.Keys.Contains(username))
                    throw new Exception($"User {username} has already been taken");

                var user = new User(username);

                _users.Add(username,user);
                _userUpdates.Add(new UserUpdate(){User = username, UserUpdateType = UserUpdateType.Login});
            }
        }

        public void UserLogout(string username)
        {
            lock (_lock)
            {
                if (_users.ContainsKey(username))
                {
                    _users.Remove(username);
                    _userUpdates.Add(new UserUpdate() { User = username, UserUpdateType = UserUpdateType.Logout });
                }
            }
        }

        public bool IsUserValid(string username)
        {
            lock(_lock) return _users.ContainsKey(username);
        }

        public void SendChatMessage(ChatMessage message)
        {
            lock (_lock)
            {
                _chatMessages.Add(message);
            }
        }

        public IEnumerable<UserUpdate> GetUserUpdates(string username)
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

        public IEnumerable<ChatMessage> GetChatMessages(string username)
        {
            lock (_lock)
            {
                var messages = _chatMessages.Skip(_users[username].ChatMessageIndex).ToList();
                _users[username].ChatMessageIndex = _chatMessages.Count;
                return messages;
            }
        }
    }
}