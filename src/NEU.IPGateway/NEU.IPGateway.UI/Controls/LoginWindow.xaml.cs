using NEU.IPGateway.Core.Models;
using NEU.IPGateway.UI.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Controls
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        public LoginWindow(string username,bool canEditUsername = true)
        {
            InitializeComponent();

            usernameBox.Text = username;
            if (!canEditUsername) usernameBox.IsReadOnly = true;
        }

        private string _message;

        public string Message 
        { 
            get => _message; 
            set
            {
                _message = value;
                messageText.Text = value;
            }
        }

        public User Result
        {
            get => new User
            {
                Username = usernameBox.Text,
                Password = passwordBox.Password
            };
            set
            {
                if (value != null)
                {
                    usernameBox.Text = value.Username;
                    passwordBox.Password = value.Password;
                }
            }
        }

        public Func<Task<bool>> LoginCallBack;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = (PasswordBox)sender;
            if (string.IsNullOrEmpty(pb.Password))
            {
                pb.Background = (VisualBrush)pb.Resources["HelpBrush"];
            }
            else
            {
                pb.Background = null;
            }
        }

        private async void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Password))
            {
                Message = Languages.I18NStringUtil.GetString("lw_empty");
                return;
            }

            if (LoginCallBack != null)
            {
                if (!await LoginCallBack()) return;
            }
            DialogResult = true;            
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            HyperlinkUtil.Open(link.NavigateUri.AbsoluteUri);
        }
    }
}
