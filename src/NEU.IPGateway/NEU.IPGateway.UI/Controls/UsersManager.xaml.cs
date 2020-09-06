using NEU.IPGateway.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
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

namespace NEU.IPGateway.UI.Controls
{
    /// <summary>
    /// Interaction logic for UsersManager.xaml
    /// </summary>
    public partial class UsersManager : ReactiveUserControl<UsersManagerViewModel>
    {
        public UsersManager()
        {
            InitializeComponent();

            ViewModel = new UsersManagerViewModel();
            this.WhenActivated(d =>
            {

                this.OneWayBind(ViewModel, vm => vm.UserViewModels, v => v.listView.ItemsSource)
                    .DisposeWith(d);

                this.BindCommand(ViewModel, vm => vm.Refresh, v => v.refreshButton)
                    .DisposeWith(d);

            });

        }

        private void newUserButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginWindow();

            dialog.ShowDialog();

            if (dialog.DialogResult == true)
            {
                string pin = "";

                // TODO: i18n it
                var isPinNeeded = MessageBox.Show("不使用PIN可能会导致恶意软件破解您的用户密码，" +
                    "为了您的安全，我们推荐您使用PIN加密您的密码，" +
                    "这可能会导致您每次连接时都需要几秒钟的时间输入PIN，" +
                    "您是否需要使用PIN来保护您的密码？", "安全提示", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (isPinNeeded == MessageBoxResult.Yes)
                {
                    var dialogPin = new PinSimpleDialog();
                    if (dialogPin.ShowDialog() == true)
                    {
                        pin = dialogPin.Result;
                    }
                }

                ViewModel.AddUser.Execute((dialog.Result, pin)).Subscribe();
            }

        }
    }
}
