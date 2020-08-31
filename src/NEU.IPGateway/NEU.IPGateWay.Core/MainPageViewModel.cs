using NEU.IPGateWay.Core.Models;
using NEU.IPGateWay.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace NEU.IPGateWay.Core
{
    public class MainPageViewModel : ReactiveObject, IDisposable
    {
        readonly ObservableAsPropertyHelper<User> _selectedUser;

        public User SelectedUser => _selectedUser.Value;

        readonly ObservableAsPropertyHelper<ConnectStatus> _connectStatus;

        public ConnectStatus ConnectStatus => _connectStatus.Value;

        public ReactiveCommand<Unit, Unit> Toggle => GlobalStatusStore.Current.Toggle;


        [Reactive]
        public bool PinRequired { get; set; } = false;

        private CompositeDisposable disposables = new CompositeDisposable();

        public MainPageViewModel()
        {

            _selectedUser = GlobalStatusStore.Current
                .WhenAnyValue(x => x.CurrentUser)
                .ToProperty(this, x => x.SelectedUser)
                .DisposeWith(disposables);

            _connectStatus = GlobalStatusStore.Current
                .WhenAnyValue(x => x.ConnectStatus)
                .ToProperty(this, x => x.ConnectStatus)
                .DisposeWith(disposables);


        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
