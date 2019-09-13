using System.Collections.Specialized;

namespace Jvh.App.ChatServer.Repo
{
    class User
    {
        public string Name { get; }
        public int MessageIndex { get; set; }
        public int UserIndex { get; set; }

        public User(string name)
        {
            Name = name;
            MessageIndex = 0;
            UserIndex = 0;
        }
    }
}