using NEU.IPGateWay.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace NEU.IPGateWay.Core
{
    public class MainPageViewModel : ReactiveObject, IDisposable
    {
        readonly ObservableAsPropertyHelper<User> _selectedUser;

        public User SelectedUser => _selectedUser.Value;

        readonly ObservableAsPropertyHelper<ConnectStatus> _connectStatus;

        public ConnectStatus ConnectStatus => _connectStatus.Value;

        public ReactiveCommand<Unit, Unit> Toggle => GlobalStatusStore.Current.Toggle;

        public ReactiveCommand<string, Unit> ContinueConnect;


        public ReactiveCommand<Unit, Unit> CancelConnect;

        [Reactive]
        public bool PinRequired { get; set; } = false;

        [Reactive]
        public bool PasswordRequired { get; set; } = false;



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

            Action<Exception> handleExceptionAction = x =>
            {
                if (x is ConnectionException ex)
                {
                    if (ex.ErrorType == ConnectionError.LostPin
                        || ex.ErrorType == ConnectionError.InvalidPin)
                    {
                        PinRequired = true;
                    }
                    else if (ex.ErrorType == ConnectionError.InvalidCredient)
                    {
                        PasswordRequired = true;
                    }
                }
                
            };

            CancelConnect = ReactiveCommand.Create(() =>
            {
                PinRequired = false;
                GlobalStatusStore.Current.ConnectStatus = ConnectStatus.Disconnected;
            });



            ContinueConnect = ReactiveCommand.CreateFromTask<string>(async (pin) =>
            {
                PinRequired = false;
                await GlobalStatusStore.Current.DoConnect.Execute(pin);
            });

            Observable.Merge(
                ContinueConnect.ThrownExceptions,
                Toggle.ThrownExceptions,
                GlobalStatusStore.Current.DoConnect.ThrownExceptions
                )
                .Throttle(TimeSpan.FromMilliseconds(100))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(handleExceptionAction)
                .DisposeWith(disposables);

            

        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
