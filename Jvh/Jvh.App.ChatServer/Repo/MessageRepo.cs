using System.Collections.Generic;
using System.Linq;
using Jvh.Service.Chat;

namespace Jvh.App.ChatServer.Repo
{
    class MessageRepo
    {
        private readonly object _lock = new object();
        private readonly List<ChatMessage> _chatMessages = new List<ChatMessage>();

        public IEnumerable<ChatMessage> GetChatMessages()
        {
            lock (_lock) return _chatMessages.ToList();
        }

        public void AddMessage(ChatMessage chatMessage)
        {
            lock(_lock) _chatMessages.Add(chatMessage);
        }
    }
}