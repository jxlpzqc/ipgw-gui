using NEU.IPGateWay.Core.Models;
using NEU.IPGateWay.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace NEU.IPGateWay.Core
{
    public class RemindConnectViewModel : ReactiveObject
    {
        public GlobalStatusStore Global { get; set; } = GlobalStatusStore.Current;

        [ObservableAsProperty]
        public bool RequirePin { get; }

        [ObservableAsProperty]
        public int RemainCloseSecond { get; }

        [ObservableAsProperty]
        public string FailMessage { get; }

        [ObservableAsProperty]
        public bool NeedClose { get; }

        [ObservableAsProperty]
        public ConnectStatus Status { get; }

        [ObservableAsProperty]
        public bool IsSuccess { get; }

        [ObservableAsProperty]
        public bool IsFail { get; }

        [ObservableAsProperty]
        public bool RequirePassword { get; }

        [Reactive]
        public bool IsNotRemindMeLater { get; set; }

        public ReactiveCommand<string, Unit> ContinueConnectWithPin { get; }

        public ReactiveCommand<(string password, string newpin), Unit> ContinueConnectWithPassword { get; }

        public ReactiveCommand<Unit, Unit> CancelConnect { get; }

        public ReactiveCommand<Unit, Unit> Connect { get; }



        public RemindConnectViewModel()
        {
            var totalTime = 5;

            this.Connect = ReactiveCommand.CreateFromObservable(
                ConnectImpl
                , this.WhenAnyValue(u => u.Global.ConnectStatus).Select(p => p == ConnectStatus.Disconnected)
            );

            this.ContinueConnectWithPin =
                ReactiveCommand.CreateFromObservable<string, Unit>(ContinueConnectWithPinImpl);

            this.ContinueConnectWithPassword =
                ReactiveCommand.CreateFromObservable<(string password, string newpin), Unit>(ContinueConnectWithPasswordImpl);

            this.CancelConnect =
                ReactiveCommand.CreateFromObservable(CancelConectImpl);


            //完成倒计时
            this.WhenAnyValue(u => u.IsSuccess)
                .Merge(this.WhenAnyValue(u => u.IsFail))
                .Where(u => u)
                .SelectMany(_ => Observable.Interval(TimeSpan.FromSeconds(1)))
                .Take(totalTime + 1)
                .Select(u =>
                {
                    return (int)(totalTime - u);
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.RemainCloseSecond, 5);

            this.WhenAnyValue(u => u.RemainCloseSecond)
                .Select(u => u == 0)
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.NeedClose);



            var mayCausedError = Observable.Merge(
                this.Connect.ThrownExceptions,
                this.ContinueConnectWithPin.ThrownExceptions,
                this.ContinueConnectWithPassword.ThrownExceptions
            );

            mayCausedError.Where(u => (u is ConnectionException))
                .Select(u =>
                {
                    var t = ((ConnectionException)u).ErrorType;
                    if (t == ConnectionError.InvalidPin || t == ConnectionError.LostPin)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .Delay(TimeSpan.FromMilliseconds(50))
                .Merge(this.ContinueConnectWithPin.IsExecuting.Select(u => false))
                .Merge(this.CancelConnect.Select(u => false))
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.RequirePin, false);


            mayCausedError.Where(u => (u is ConnectionException))
                .Select(u =>
                {
                    var t = ((ConnectionException)u).ErrorType;
                    if (t == ConnectionError.InvalidCredient)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }).ToPropertyEx(this, x => x.RequirePassword, false);

            mayCausedError.Select(u => u.Message)
                .ToPropertyEx(this, x => x.FailMessage);

            mayCausedError.Where(u => (u is ConnectionException))
                .Select(u =>
                {
                    var t = ((ConnectionException)u).ErrorType;
                    if (t == ConnectionError.Unclear || t == ConnectionError.NotConnected)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                })
                .ToPropertyEx(this, x => x.IsFail, false);

            this.WhenAnyValue(u => u.Global.ConnectStatus)
                .Select(u => u == ConnectStatus.Connected)
                .ToPropertyEx(this, x => x.IsSuccess);

            this.WhenAnyValue(u => u.Global.ConnectStatus)
                .ToPropertyEx(this, x => x.Status);


        }

        private IObservable<Unit> CancelConectImpl()
        {
            return Observable.StartAsync(async () =>
            {
                Global.ConnectStatus = ConnectStatus.Disconnected;
            });
        }

        private IObservable<Unit> ContinueConnectWithPasswordImpl((string password, string newpin) input)
        {
            return Observable.StartAsync(async () =>
            {
                var user = Global.CurrentUser;
                var userService = Locator.Current.GetService<IUserStorageService>();
                await userService.ResetUserPassword(user.Username, input.password, input.newpin);
            });
        }

        private IObservable<Unit> ContinueConnectWithPinImpl(string pin)
        {
            return Observable.StartAsync(async () =>
            {
                await this.Global.DoConnect.Execute(pin);
            });
        }

        private IObservable<Unit> ConnectImpl()
        {
            return Observable.StartAsync(async () =>
            {

                //TODO Remind me later
                await this.Global.Connect.Execute();
            });

        }
    }
}
