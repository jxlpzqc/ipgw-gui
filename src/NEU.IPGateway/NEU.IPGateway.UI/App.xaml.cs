using Hardcodet.Wpf.TaskbarNotification;
using NEU.IPGateway.UI.Services;
using NEU.IPGateway.UI.Views;
using NEU.IPGateWay.Core;
using NEU.IPGateWay.Core.Services;
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
            Locator.CurrentMutable.Register<IInternetGateWayService>(() => new InternetGateWayService());
            
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (!Environment.GetCommandLineArgs().Contains("/s"))
            {
                ShowMainWindow();
            }

            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
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
