using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;

namespace NEU.IPGateWay.Core
{
    public class RemindConnectViewModel : ReactiveObject
    {
        public GlobalStatusStore Global { get; set; } = GlobalStatusStore.Current;

        public ReactiveCommand<Unit,Unit> Connect => Global.Connect;

        [Reactive]
        public bool RequirePin { get; set; }

        [Reactive]
        public bool Disconnect { get; set; }

        [ObservableAsProperty]
        public int RemainCloseSecond { get; }        



        public RemindConnectViewModel()
        {


            


        }





    }
}
