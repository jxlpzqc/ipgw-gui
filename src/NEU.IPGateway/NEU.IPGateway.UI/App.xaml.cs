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
                
        public void ShowMainWindow()
        {
            bool flag = true;
            foreach (var item in Windows)
            {
                if (item is MainWindow win)
                {
                    win.Show();
                    win.Focus();                    
                    flag = false;
                }
            }
            if (flag)
                new MainWindow().Show();
        }


        public void HideMainWindow()
        {
            foreach (var item in Windows)
            {
                if (item is MainWindow win)
                {
                    win.Close();
                }
            }
        }

        private bool IsMainWindowTopmost()
        {
            bool res = false;

            Dispatcher.Invoke(() =>
            {
                foreach (var item in Windows)
                {
                    if (item is MainWindow win)
                    {
                        if (win.IsActive) res = true;
                        else res = false;
                        return;
                    }
                }
                res = false;

            });
            return res;
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
                    var result = await Locator.Current.GetService<IInternetGatewayService>().Test();
                    if (result.connected && !result.logedin && !IsMainWindowTopmost())
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
                        if (!result.connected) GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.DisconnectedFromNetwork;
                        else if (result.logedin) GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Connected;
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
