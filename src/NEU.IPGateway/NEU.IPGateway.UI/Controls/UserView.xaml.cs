using NEU.IPGateWay.Core;
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


                this.BindCommand(ViewModel,
                    x => x.Delete,
                    v => v.deleteUserMenu)
                    .DisposeWith(d);

                ViewModel.SetCurrent.ThrownExceptions.Subscribe((e) =>
                {
                    // handle the raised exception.
                    MessageBox.Show(e.Message);
                });

                ViewModel.DisposeWith(d);


            });
        }

        private void moreBtn_Click(object sender, RoutedEventArgs e)
        {
            menuPop.IsOpen = true;
        }

        private void showPasswordBtn_Click(object sender, RoutedEventArgs e)
        {
            string pin = "";
            // TODO i18n
            if (!ViewModel.IsPasswordShown && ViewModel.HasPin)
            {
                var dialog = new PinSimpleDialog("输入密码以显示PIN值");

                if (dialog.ShowDialog() == true)
                {
                    pin = dialog.Result;
                }
                else
                {
                    return;
                }
            }

            ViewModel.TogglePasswordShown.Execute(pin).Subscribe();

        }
    }
}
