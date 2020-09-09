using NEU.IPGateway.Core.Services;
using ReactiveUI;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ReactiveWindow<Core.SettingsViewModel>
    {
        public SettingsWindow()
        {
            InitializeComponent();
            this.ViewModel = new Core.SettingsViewModel();
            this.WhenActivated(d =>
            {
                this.Bind(ViewModel,
                    x => x.Setting.Language,
                    v => v.languageCombo.SelectedIndex,
                    vmToViewConverter: x => x == "en-us" ? 1 : 0,
                    viewToVmConverter: x => x == 1 ? "en-us" : "zh-cn")
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    x => x.Setting.LaunchWhenStartup,
                    v => v.launchCb.IsChecked)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    x => x.Setting.RemindConnect,
                    v => v.autoRemindCb.IsChecked)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    x => x.Setting.AutoConnect,
                    v => v.autoConnectCb.IsChecked)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    x => x.Setting.AutoUploadCrash,
                    v => v.autoUploadCrashCb.IsChecked)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    x => x.Setting.AutoUpdate,
                    v => v.updateCb.IsChecked)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    x => x.Setting.SilentUpdate,
                    v => v.silentUpdateCb.IsChecked)
                    .DisposeWith(d);

                


            });
            InitializeVersionInfo();
        }

        private async void InitializeVersionInfo()
        {
            uiVersionText.Content = Assembly.GetAssembly(typeof(SettingsWindow)).GetName().Version.ToString();
            coreVersionText.Content = Assembly.GetAssembly(typeof(Core.SettingsViewModel)).GetName().Version.ToString();
            driverVersionText.Content = await (Locator.Current.GetService<IInternetGatewayService>()).GetDriverVersion();
#if core31
            targetPlatformText.Content = RuntimeInformation.FrameworkDescription
               + " on "
               + RuntimeInformation.OSDescription.ToString()
               + (Environment.Is64BitProcess ? " <64 bit> " : " <32 bit> ");

#endif

#if net461
            targetPlatformText.Content = ".NET Framework CLR " + Environment.Version
                + " on " 
                + Environment.OSVersion.ToString()
                + (Environment.Is64BitProcess ? " <64 bit> " : " <32 bit> ");
#endif

        }
    }
}
