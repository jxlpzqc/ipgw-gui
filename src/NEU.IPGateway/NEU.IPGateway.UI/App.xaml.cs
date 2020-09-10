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
using System.Reactive.Linq;

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

        #region AutoLaunch Registry CRUD Part

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

        #endregion

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
                    win.Topmost = true;
                    win.Topmost = false;
                    win.Focus();
                    flag = false;
                }
            }
            if (flag)
                new MainWindow().Show();

            UpdateStatus();
        }

        private Task UpdateStatus()
        {
            var result = new TaskCompletionSource<bool>();
            Dispatcher.Invoke(async () =>
            {
                await GlobalStatusStore.Current.Test.Execute();
                result.SetResult(true);
            });
            return result.Task;
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
            // application singleton
            Process currentProcess = Process.GetCurrentProcess();
            var processList = Process.GetProcessesByName(currentProcess.ProcessName);
            if (processList.Length > 1)
            {
                this.Shutdown();
            }

            InitializeStartupChange();
            await InitializeLanguageChange();
            System.Net.NetworkInformation.NetworkChange.NetworkAddressChanged += NetworkChange_NetworkAddressChanged;

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");

            if (!Environment.GetCommandLineArgs().Contains("/s"))
            {
                ShowMainWindow();
            }

            await UpdateStatusAndRemind();
        }

        private async Task UpdateStatusAndRemind()
        {
            await UpdateStatus();
            if (!GlobalStatusStore.Current.Setting.RemindConnect) return;

            try
            {

                if (GlobalStatusStore.Current.ConnectStatus == Core.Models.ConnectStatus.Disconnected
                    && !IsMainWindowActive())
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

            }
            catch (Exception ex)
            { }

        }

        private void NetworkChange_NetworkAddressChanged(object sender, EventArgs e)
        {
            UpdateStatusAndRemind();
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
