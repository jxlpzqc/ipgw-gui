using NEU.IPGateWay.Core;
using ReactiveUI;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;

namespace NEU.IPGateway.UI.Controls
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView : ReactiveUserControl<UserViewModel>
    {
        public UserView()
        {
            InitializeComponent();
            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    x => x.User,
                    v => v.usernameText.Text,
                    (x) => (x?.Username))
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.IsCurrent,
                    v => v.isCurrentUserText.Visibility)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    x => x.IsPasswordShown,
                    v => v.hidePasswordText.Visibility,
                    x => x ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.IsPasswordShown,
                    v => v.passwordText.Visibility)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.IsPasswordShown,
                    v => v.showPasswordBtn.Content,
                    x => x ? "隐藏密码" : "显示密码")
                    .DisposeWith(d);



                this.OneWayBind(ViewModel,
                    x => x.User.Password,
                    v => v.passwordText.Text)
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    x => x.SetCurrent,
                    v => v.setDefaultBtn)
                    .DisposeWith(d);

                ViewModel.ChangePin.ThrownExceptions.Merge(
                    ViewModel.SetCurrent.ThrownExceptions)
                .Subscribe((e) =>
                {
                    // handle the raised exception.
                    MessageBox.Show(e.Message);
                }).DisposeWith(d);


                ViewModel.DisposeWith(d);

                


            });
        }

        private void moreBtn_Click(object sender, RoutedEventArgs e)
        {
            menuPop.IsOpen = true;
        }

        private async void showPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            string pin = "";
            // TODO i18n
            if (!ViewModel.IsPasswordShown && ViewModel.HasPin)
            {
                var dialog = new PinSimpleDialog("输入PIN以显示密码");

                if (dialog.ShowDialog() == true)
                {
                    pin = dialog.Result;
                }
                else
                {
                    return;
                }
            }

            await ViewModel.TogglePasswordShown.Execute(pin);

        }

        private async void deleteUserMenu_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("您确认要删除该用户吗？",
                "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                await ViewModel.Delete.Execute();
            }

        }

        private async void editPasswordMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginWindow(ViewModel.User.Username, false);
            dialog.Message = "请输入以修改保存的密码";
            if (dialog.ShowDialog() == true)
            {
                var res = MessageBox.Show("是否使用PIN保护您设置的新密码？",
                    "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
                var pin = "";
                if(res == MessageBoxResult.Yes)
                {
                    var pinD = new PinSimpleDialog("设置新的PIN值");
                    if (pinD.ShowDialog() == true)
                    {
                        pin = pinD.Result;
                    }
                }

                await ViewModel.ChangePassword.Execute((dialog.Result.Password,pin));
            }
        }

        private async void editPinMenu_Click(object sender, RoutedEventArgs e)
        {
            var oldPinDialog = new PinSimpleDialog("输入旧的PIN");
            var newPinDialog = new PinSimpleDialog("输入新的PIN");
            if (oldPinDialog.ShowDialog() == true && newPinDialog.ShowDialog() == true)
            {
                await ViewModel.ChangePin.Execute((oldPinDialog.Result, newPinDialog.Result));
            }
        }

        private void verifyPinMenu_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
