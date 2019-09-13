using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;

namespace Jvh.Chat.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ThemedWindow
    {
        private Client client = new Client();

        public MainWindow()
        {
            InitializeComponent();
            client = new Client();
        }

        private void Button_Click_Login(object sender, RoutedEventArgs e)
        {
           
            client.Connect();
            client.Login(TextBoxUsername.Text);
            client.OnChatMessage +=  OnChatMessage;
            client.OnUserUpdate += OnUserUpdate;
        }

        private void OnUserUpdate(object sender, UserUpdate e)
        {
            TextBoxChatMain.Dispatcher.Invoke(() =>
            {
                if (e.UserUpdateType == UserUpdateType.Login)
                {
                    TextBoxChatMain.AppendText($"New User has arrived: {e.User}\r\n");
                }
                else
                {
                    TextBoxChatMain.AppendText($"User {e.User} has left the chat.\r\n");
                }
            });
        }

        private void OnChatMessage(object sender, ChatMessage e)
        {
            TextBoxChatMain.Dispatcher.Invoke(() =>
            {
                TextBoxChatMain.AppendText($"{e.Timestamp.ToDateTime():G} [{e.From}]  {e.Message}\r\n");
            });
        }

        private void Button_Click_Logout(object sender, RoutedEventArgs e)
        {
            client.Logoff();
            client.Disconnect();
            
        }

        private void Button_Click_SendMessage(object sender, RoutedEventArgs e)
        {
            client.SendMessage(TextBoxChatInput.Text);
            TextBoxChatInput.Clear();
        }
    }
}
