using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Jvh.Service.Chat;

namespace Jvh.App.ChatClient.View
{
    /// <summary>
    /// Interaction logic for ChatRoom.xaml
    /// </summary>
    public partial class ChatRoom : UserControl
    {
        private ChatRoomClient _client = new ChatRoomClient();
        private IDisposable _messageSubscription;
        private IDisposable _userUpdateSubscription;

        public ChatRoom()
        {
            InitializeComponent();

            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            _client.Connect();
        }

        private void Button_Click_SendMessage(object sender, RoutedEventArgs e)
        {
            _client.SendMessage(TextBoxChatInput.Text);
            TextBoxChatInput.Clear();
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
            _client.Login(TextBoxUsername.Text);
            _messageSubscription = _client.ChatMessageObservable.ObserveOnDispatcher().Subscribe(OnNextChatMessage);
            _userUpdateSubscription = _client.UserUpdateObservable.ObserveOnDispatcher().Subscribe(OnNextUserUpdate);
        }

        private void OnNextUserUpdate(UserUpdate userUpdate)
        {
            if (userUpdate.UserUpdateType == UserUpdateType.Login)
            {
                TextBoxChatMain.AppendText($"Welcome {userUpdate.User} to the chatroom.\r\n");
            }
            else
            {
                TextBoxChatMain.AppendText($"{userUpdate.User} has left the chatroom.\r\n");

            }
        }

        private void OnNextChatMessage(ChatMessage chatMessage)
        {
            TextBoxChatMain.AppendText($"[{chatMessage.Timestamp.ToDateTime().ToShortTimeString()}] {chatMessage.From} says: {chatMessage.Message}\r\n");
        }

        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            _messageSubscription.Dispose();
            _userUpdateSubscription.Dispose();
            _client.Logoff();
        }
    }
}
