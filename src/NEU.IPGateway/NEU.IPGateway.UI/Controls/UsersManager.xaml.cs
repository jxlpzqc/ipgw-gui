using NEU.IPGateway.Core;
using NEU.IPGateway.UI.Languages;
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

                var isPinNeeded = MessageBox.Show(I18NStringUtil.GetString("um_pin_hint"), I18NStringUtil.GetString("warning"), MessageBoxButton.YesNo, MessageBoxImage.Question);

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
