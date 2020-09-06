using NEU.IPGateway.Core.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace NEU.IPGateway.Core
{
    public class MainPageViewModel : ReactiveObject, IActivatableViewModel
    {

        private GlobalStatusStore Global { get; set; } = GlobalStatusStore.Current;

        [Reactive]
        public User User { get; set; }

        [Reactive]
        public ConnectStatus ConnectionStatus { get; set; }

        [ObservableAsProperty]
        public User SelectedUser { get; }

        [ObservableAsProperty]
        public ConnectStatus ConnectStatus { get; }

        [ObservableAsProperty]
        public bool PinRequired { get; }

        [ObservableAsProperty]
        public bool PasswordRequired { get; }

        [ObservableAsProperty]
        public double UsedData { get; }

        [ObservableAsProperty]
        public double UnusedData { get; }

        [ObservableAsProperty]
        public TimeSpan TotalTime { get; }

        [ObservableAsProperty]
        public TimeSpan CurrentTime { get; }

        [ObservableAsProperty]
        public bool RemainMoney { get; }

        [ObservableAsProperty]
        public string AlertMessage { get; }

        [ObservableAsProperty]
        public bool AlertRequired { get; }

        public ViewModelActivator Activator { get; }

        public ReactiveCommand<Unit, Unit> Toggle { get; }

        public ReactiveCommand<string, Unit> ContinueConnect { get; }

        public ReactiveCommand<Unit, Unit> CancelConnect { get; }


        public MainPageViewModel()
        {
            Activator = new ViewModelActivator();

            this.CancelConnect = ReactiveCommand.Create(() =>
            {
                Global.ConnectStatus = ConnectStatus.Disconnected;
            });

            this.ContinueConnect = ReactiveCommand.CreateFromTask<string>(async (pin) =>
            {
                await Global.DoConnect.Execute(pin);
            });

            this.Toggle = ReactiveCommand.CreateFromTask(async () =>
            {
                await Global.Toggle.Execute();
            });

            this.WhenActivated(d =>
            {

                this.WhenAnyValue(x => x.Global.CurrentUser)
                    .ToPropertyEx(this, x => x.SelectedUser)
                    .DisposeWith(d);

                this.WhenAnyValue(x => x.Global.ConnectStatus)
                    .ToPropertyEx(this, x => x.ConnectStatus)
                    .DisposeWith(d);

                var errorStream = Observable.Merge(
                    this.ContinueConnect.ThrownExceptions,
                    this.Toggle.ThrownExceptions
                    )
                    .Where(u => u is ConnectionException);

                errorStream
                    .Select(u => ((ConnectionException)u).ErrorType)
                    .Where(u => u == ConnectionError.LostPin || u == ConnectionError.InvalidPin)
                    .Select(u => true)
                    .Delay(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                    .Merge(this.CancelConnect.Select(_ => false))
                    .Merge(this.ContinueConnect.IsExecuting.Select(_ => false))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.PinRequired)
                    .DisposeWith(d);


                errorStream
                    .Select(u => ((ConnectionException)u).ErrorType)
                    .Where(u => u == ConnectionError.InvalidCredient)
                    .Select(u => true)
                    .Delay(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                    .Merge(this.CancelConnect.Select(_ => false))
                    .Merge(this.ContinueConnect.IsExecuting.Select(_ => false))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.PasswordRequired)
                    .DisposeWith(d);


                var errorFromDriver = Observable.Merge(
                    this.ContinueConnect.ThrownExceptions,
                    this.Toggle.ThrownExceptions
                    ).Where(u => 
                        ((u is ConnectionException ce) && ce.ErrorType == ConnectionError.Unclear) ||
                        (!(u is ConnectionException)))
                    .Select(u => u.Message);


                errorFromDriver
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.AlertMessage)
                    .DisposeWith(d);


                errorFromDriver
                    .Select(u => true)
                    .Merge(
                            errorFromDriver
                                .Delay(TimeSpan.FromMilliseconds(100))
                                .Select(u => false)
                    )
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.AlertRequired, false)
                    .DisposeWith(d);


                errorFromDriver
                    .Subscribe(x =>
                    {
                        this.Global.ConnectStatus = ConnectStatus.Disconnected;
                    })
                    .DisposeWith(d);


            });

        }


    }
}
