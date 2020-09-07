using NEU.IPGateway.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;

namespace NEU.IPGateway.Core
{
    public class SettingsViewModel : ReactiveObject, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; }

        public GlobalStatusStore Global { get; }

        [ObservableAsProperty]
        public Setting Setting { get; }

        public SettingsViewModel()
        {
            Activator = new ViewModelActivator();
            this.Global = GlobalStatusStore.Current;

            this.WhenActivated(d =>
            { 
                this.WhenAnyValue(x => x.Global.Setting)
                    .ToPropertyEx(this, x => x.Setting)
                    .DisposeWith(d);
            });


        }


    }
}
