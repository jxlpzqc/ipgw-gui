using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace NEU.IPGateway.Core
{
    public class MainPageViewModel : ReactiveObject, IActivatableViewModel
    {

        private GlobalStatusStore Global { get; set; } = GlobalStatusStore.Current;

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

        [ObservableAsProperty]
        public AccountInfo AccountInfo { get; }

        public ViewModelActivator Activator { get; }

        public ReactiveCommand<Unit, Unit> Toggle { get; }

        public ReactiveCommand<string, Unit> ContinueConnectWithPin { get; }

        public ReactiveCommand<string, Unit> ContinueConnectWithPassword { get; }

        public ReactiveCommand<Unit, Unit> CancelConnect { get; }
        
        public ReactiveCommand<string, Unit> ForceDisconnect { get; }

        public MainPageViewModel()
        {
            Activator = new ViewModelActivator();

            this.ForceDisconnect = ReactiveCommand.CreateFromTask<string,Unit>(ForceDisconnectImpl);

            this.CancelConnect = ReactiveCommand.Create(() =>
            {
                Global.ConnectStatus = ConnectStatus.Disconnected;
            });

            this.ContinueConnectWithPin = ReactiveCommand.CreateFromTask<string>(async (pin) =>
            {
                await Global.DoConnect.Execute(pin);
            });

            this.ContinueConnectWithPassword = ReactiveCommand.CreateFromTask<string>(async (password) =>
            {
                IUserStorageService service = Locator.Current.GetService<IUserStorageService>();
                try
                {
                    await service.ResetUserPassword(Global.CurrentUser.Username, password, "");
                }
                catch
                {
                    throw new Exception("新密码保存错误");
                }
                await Global.DoConnect.Execute("");
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

                var errorSource = Observable.Merge(
                    this.ContinueConnectWithPin.ThrownExceptions,
                    this.Toggle.ThrownExceptions,
                    this.ContinueConnectWithPassword.ThrownExceptions
                    );

                var errorStream = errorSource
                    .Where(u => u is ConnectionException);


                this.WhenAnyValue(x => x.ConnectStatus)
                    .Where(u => u == ConnectStatus.Connected)
                    .Select(u => Unit.Default)
                    .Merge(Observable.Interval(TimeSpan.FromMinutes(10)).Select(p => Unit.Default))
                    .SelectMany(async _ => await Locator.Current.GetService<IInternetGatewayService>().GetAccountInfo())
                    .Catch(Observable.Return(new AccountInfo
                    {
                        Name = "获取失败",
                        Plan = "N/A",
                        UsedTime = TimeSpan.FromMinutes(0)
                    }))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.AccountInfo);

                errorStream
                    .Select(u => ((ConnectionException)u).ErrorType)
                    .Where(u => u == ConnectionError.LostPin || u == ConnectionError.InvalidPin)
                    .Select(u => true)
                    .Delay(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                    .Merge(this.CancelConnect.Select(_ => false))
                    .Merge(this.ContinueConnectWithPin.IsExecuting.Select(_ => false))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.PinRequired)
                    .DisposeWith(d);


                errorStream
                    .Select(u => ((ConnectionException)u).ErrorType)
                    .Where(u => u == ConnectionError.InvalidCredient)
                    .Select(u => true)
                    .Delay(TimeSpan.FromMilliseconds(100), RxApp.MainThreadScheduler)
                    .Merge(this.CancelConnect.Select(_ => false))
                    .Merge(this.ContinueConnectWithPassword.IsExecuting.Select(_ => false))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.PasswordRequired)
                    .DisposeWith(d);


                var errorFromDriver = errorSource.Where(u => 
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

        private async Task<Unit> ForceDisconnectImpl(string password)
        {
            await Locator.Current.GetService<Services.IInternetGatewayService>().ForceDisconnect(SelectedUser.Username, password);
            Global.ConnectStatus = ConnectStatus.Disconnected;
            return Unit.Default;
        }
    }
}
