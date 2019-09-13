using System;
using System.Collections.Generic;

namespace Jvh.App.ChatServer.Repo
{
    class UserRepo
    {
        private readonly object _lock = new object();
        private readonly Dictionary<string,User> _users = new Dictionary<string, User>();

        public void AddUser(string username)
        {
            lock (_lock)
            {
                if(_users.ContainsKey(username))
                    throw new Exception("User already exists");

                var user = new User(username);
                _users.Add(username,user);
            }
        }

        public void RemoveUser(string username)
        {
            lock (_lock)
            {
                if (_users.ContainsKey(username))
                    _users.Remove(username);
            }
        }

        public int GetMessageIndexForUser(string username)
        {
            lock (_lock) return _users[username].MessageIndex;
        }
    }
}