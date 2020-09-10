using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateway.Core
{
    public class ConnectionTestViewModel : ReactiveObject
    {
        [ObservableAsProperty]
        public bool IsRunning { get; }

        public ReactiveCommand<Unit, Unit> Test { get; }

        [Reactive]
        public bool IsConnected { get; set; }

        [Reactive]
        public bool IsLogined { get; set; }

        [ObservableAsProperty]
        public bool IsError { get; }

        public ConnectionTestViewModel()
        {
            Test = ReactiveCommand.CreateFromTask(TestImpl);
            this.Test.ThrownExceptions
                .Select(_ => true)
                .ToPropertyEx(this, x => x.IsError);

            this.Test.IsExecuting
                .ToPropertyEx(this, x => x.IsRunning);

            try
            {
                this.Test.Execute().Subscribe();
            }
            catch { }
        }

        private async Task TestImpl()
        {
            var testinfo = await Locator.Current.GetService<Services.IInternetGatewayService>().Test();
            IsConnected = testinfo.connected;
            IsLogined = testinfo.logedin;

        }
    }
}
