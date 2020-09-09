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
using System.Diagnostics;
using Microsoft.Win32;

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

        public void SetLanguage(string language)
        {
            this.Resources.MergedDictionaries[0] = LoadComponent(new Uri(@"Languages\" + language + @"\CommonStrings.xaml", UriKind.Relative)) as ResourceDictionary;
        }

        private Task<bool> InitializeLanguageChange()
        {
            TaskCompletionSource<bool> source = new System.Threading.Tasks.TaskCompletionSource<bool>(); 
            GlobalStatusStore.Current.WhenAnyValue(x => x.Setting.Language)
                .Subscribe(lan =>
                {
                    source?.TrySetResult(true);
                    if (lan == "en-us")
                    {
                        ((App)App.Current).SetLanguage("en-us");
                    }
                    else
                    {
                        ((App)App.Current).SetLanguage("zh-cn");
                    }
                });
            return source.Task;
        }

        private void SetThisApplicationStartup(bool onOff)
        {
            string appName = "IPGWApplication";
            string appPath = $"\"{Process.GetCurrentProcess().MainModule.FileName}\" /s";
            SetAutoStart(onOff, appName, appPath);
        }

        private void SetAutoStart(bool onOff, string appName, string appPath)
        {
            if (!IsExistKey(appName) && onOff)
            {
                SetRegistryStartup(onOff, appName, appPath);
            }
            else if (IsExistKey(appName) && !onOff)
            {
                SetRegistryStartup(onOff, appName, appPath);
            }
        }

        private bool IsExistKey(string keyName)
        {
            try
            {
                bool isExist = false;
                RegistryKey currentUser = Registry.CurrentUser;
                RegistryKey startup = currentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (startup == null)
                {
                    return isExist;
                }
                string[] startupItems = startup.GetValueNames();
                foreach (string startupItem in startupItems)
                {
                    if (startupItem.ToUpper() == keyName.ToUpper())
                    {
                        isExist = true;
                        return isExist;
                    }
                }
                return isExist;

            }
            catch
            {
                return false;
            }
        }

        private void SetRegistryStartup(bool isStart, string appName, string path)
        {

            RegistryKey currentUser = Registry.CurrentUser;
            RegistryKey key = currentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            if (key == null)
            {
                currentUser.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
            }

            if (isStart)
            {
                key.SetValue(appName, path);
                key.Close();
            }
            else
            {
                string[] keyNames = key.GetValueNames();
                foreach (string keyName in keyNames)
                {
                    if (keyName.ToUpper() == appName.ToUpper())
                    {
                        key.DeleteValue(appName);
                        key.Close();
                    }
                }
            }
        }

        private void InitializeStartupChange()
        {
            GlobalStatusStore.Current.WhenAnyValue(x => x.Setting.LaunchWhenStartup)
                .Subscribe(startup =>
                {
                    SetThisApplicationStartup(startup);
                });
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

            UpdateState();
        }

        private async void UpdateState()
        {
            Dispatcher.Invoke(() =>
            {
                GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Checking;
            });
            var result = await Locator.Current.GetService<IInternetGatewayService>().Test();
            
            Dispatcher.Invoke(() =>
            {
                if (!result.connected) GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.DisconnectedFromNetwork;
                else if (result.logedin) GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Connected;
                else GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Disconnected;
            });
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

        private bool IsMainWindowActive()
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
            Locator.CurrentMutable.Register<ISettingStorageService>(() => new SettingStorageService());
            Driver.Register.Regist();
        }

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Environment.GetCommandLineArgs().Contains("/s"))
            {
                ShowMainWindow();
            }
            InitializeStartupChange();
            await InitializeLanguageChange();
            UpdateStateAndRemind();
            System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

        }

        private void UpdateStateAndRemind()
        {
            if (!GlobalStatusStore.Current.Setting.RemindConnect) return;
            Task.Run(async () =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        GlobalStatusStore.Current.ConnectStatus = Core.Models.ConnectStatus.Checking;
                    });
                    var result = await Locator.Current.GetService<IInternetGatewayService>().Test();
                    if (result.connected && !result.logedin && !IsMainWindowActive())
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
                            if (flag) new RemindConnectPopupWindow().Show();
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
