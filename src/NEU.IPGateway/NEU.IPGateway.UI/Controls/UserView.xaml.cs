using NEU.IPGateway.Core;
using NEU.IPGateway.UI.Languages;
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
                    x => x ? I18NStringUtil.GetString("hide_password") : I18NStringUtil.GetString("show_password"))
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.HasPin,
                    v => v.editPinMenu.Visibility)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    x => x.HasPin,
                    v => v.addPinMenu.Visibility,
                    x => x ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                   x => x.HasPin,
                   v => v.deletePinMenu.Visibility)
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
                    MessageBox.Show(I18NStringUtil.GetString(e.Message));
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

            if (!ViewModel.IsPasswordShown && ViewModel.HasPin)
            {
                var dialog = new PinSimpleDialog(I18NStringUtil.GetString("uv_input_pin_for_pwd"));

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
            var res = MessageBox.Show(I18NStringUtil.GetString("uv_delete_user_confirm"),
                I18NStringUtil.GetString("warning"), MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                await ViewModel.Delete.Execute();
            }

        }

        private async void editPasswordMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginWindow(ViewModel.User.Username, false);
            dialog.Message = I18NStringUtil.GetString("uv_reset_password");
            if (dialog.ShowDialog() == true)
            {
                var res = MessageBox.Show(I18NStringUtil.GetString("uv_reset_password_pin"),
                   I18NStringUtil.GetString("warning"), MessageBoxButton.YesNo, MessageBoxImage.Question);
                var pin = "";
                if (res == MessageBoxResult.Yes)
                {
                    var pinD = new PinSimpleDialog(I18NStringUtil.GetString("uv_set_new_pin"));
                    if (pinD.ShowDialog() == true)
                    {
                        pin = pinD.Result;
                    }
                }

                await ViewModel.ChangePassword.Execute((dialog.Result.Password, pin));
            }
        }

        private async void editPinMenu_Click(object sender, RoutedEventArgs e)
        {
            var oldPinDialog = new PinSimpleDialog(I18NStringUtil.GetString("uv_old_pin"));
            var newPinDialog = new PinSimpleDialog(I18NStringUtil.GetString("uv_set_new_pin"));
            if (oldPinDialog.ShowDialog() == true && newPinDialog.ShowDialog() == true)
            {
                await ViewModel.ChangePin.Execute((oldPinDialog.Result, newPinDialog.Result));
            }
        }

        private async void addPinMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PinSimpleDialog(I18NStringUtil.GetString("uv_set_new_pin"));
            if (dialog.ShowDialog() == true)
            {
                await ViewModel.ChangePin.Execute(("", dialog.Result));
            }
        }

        private async void deletePinMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PinSimpleDialog(I18NStringUtil.GetString("uv_old_pin"));
            if (dialog.ShowDialog() == true)
            {
                await ViewModel.ChangePin.Execute((dialog.Result, ""));
            }
        }
    }
}
