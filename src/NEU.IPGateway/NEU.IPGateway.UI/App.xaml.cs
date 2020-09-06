using Hardcodet.Wpf.TaskbarNotification;
using NEU.IPGateway.UI.Services;
using NEU.IPGateway.UI.Views;
using NEU.IPGateway.Core;
using NEU.IPGateway.Core.Services;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace NEU.IPGateway.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private TaskbarIcon notifyIcon;

        public App()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
            InitializeService();

        }

        private MainWindow _mainWindow;

        public void ShowMainWindow()
        {
            if (_mainWindow == null) _mainWindow = new MainWindow();
            _mainWindow.Closing += (s, e) =>
            {
                ((Window)s).Hide();
                e.Cancel = true;
            };
            _mainWindow.Show();
        }


        public void HideMainWindow()
        {
            
            _mainWindow?.Hide();

        }

        private void InitializeService()
        {
            Locator.CurrentMutable.Register<IUserStorageService>(() => new UserStorageService());
            Driver.Register.Regist();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Environment.GetCommandLineArgs().Contains("/s"))
            {
                ShowMainWindow();
            }

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
            
            UpdateStateAndRemind();
            System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;
        }

        private void UpdateStateAndRemind()
        {
            Task.Run(async () =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Checking;
                    });
                    var (connected, logedin) = await Locator.Current.GetService<IInternetGatewayService>().GetInfo();
                    if (connected && !logedin)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var flag = true;
                            foreach (var win in Windows)
                            {
                                if (win is RemindConnectPopupWindow rwin)
                                {
                                    rwin.Show();
                                    rwin.Focus();
                                    flag = false;
                                    break;
                                }
                            }
                            if(flag) new RemindConnectPopupWindow().Show();
                        });
                    }
                    Dispatcher.Invoke(() =>
                    {
                        if (!connected) GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.DisconnectedFromNetwork;
                        else if (logedin) GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Connected;
                        else GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Disconnected;
                    });
                    
                }
                catch (Exception ex)
                { }
            });
        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            UpdateStateAndRemind();  
        }

        private void ShowItemMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Shutdown();
        }

        private void ShowSettingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            new Views.SettingsWindow().Show();

        }

        private void TaskbarIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            ShowMainWindow();
        }
    }
}
