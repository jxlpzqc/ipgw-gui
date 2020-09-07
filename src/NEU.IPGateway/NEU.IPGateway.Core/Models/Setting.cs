using NEU.IPGateway.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Text;

namespace NEU.IPGateway.Core.Models
{
    [Serializable]
    public class Setting : ReactiveObject
    {
        [Reactive]
        public string Language { get; set; } = "zh-cn";

        [Reactive]
        public bool LaunchWhenStartup { get; set; } = true;

        [Reactive]
        public bool RemindConnect { get; set; } = true;

        [Reactive]
        public bool AutoConnect { get; set; } = true;

        [Reactive]
        public bool RemindDisconnect { get; set; } = true;

        [Reactive]
        public string EncryptionAlgorithm { get; set; } = "WINDPAPI";

        [Reactive]
        public bool AutoUploadCrash { get; set; } = true;

        [Reactive]
        public bool AutoUpdate { get; set; } = true;

        [Reactive]
        public bool SilentUpdate { get; set; } = true;

        public Setting()
        {
            this.PropertyChanged += Setting_PropertyChanged;
        }

        private async void Setting_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var settingService = Locator.Current.GetService<ISettingStorageService>();
            await settingService.SaveSetting(this);            
        }
    }
}
