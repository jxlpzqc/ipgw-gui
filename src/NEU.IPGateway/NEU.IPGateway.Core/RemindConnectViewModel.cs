﻿using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace NEU.IPGateway.Core
{
    public class RemindConnectViewModel : ReactiveObject,IActivatableViewModel
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

        [Reactive]
        public bool IsNotRemindMeLater { get; set; } = false;

        public ReactiveCommand<string, Unit> ContinueConnectWithPin { get; }

        public ReactiveCommand<Unit, Unit> CancelConnect { get; }

        public ReactiveCommand<Unit, Unit> Connect { get; }

        public ViewModelActivator Activator { get; }

        public RemindConnectViewModel()
        {
            Activator = new ViewModelActivator();

            this.Connect = ReactiveCommand.CreateFromObservable(
                    ConnectImpl
                    , this.WhenAnyValue(u => u.Status).Select(p => p == ConnectStatus.Disconnected)
                );

            this.ContinueConnectWithPin =
                ReactiveCommand.CreateFromObservable<string, Unit>(ContinueConnectWithPinImpl);

            this.CancelConnect =
                ReactiveCommand.CreateFromObservable(CancelConectImpl);


            this.WhenActivated((d) =>
            {
                var totalTime = 5;                

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
                    .ToPropertyEx(this, x => x.RemainCloseSecond, 5)
                    .DisposeWith(d);

                this.WhenAnyValue(u => u.RemainCloseSecond)
                    .Select(u => u == 0)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.NeedClose)
                    .DisposeWith(d);

                this.WhenAnyValue(u => u.IsNotRemindMeLater)
                    .Subscribe(x =>
                    {
                        this.Global.Setting.AutoConnect = x;
                    })
                    .DisposeWith(d);


                var mayCausedError = Observable.Merge(
                    this.Connect.ThrownExceptions,
                    this.ContinueConnectWithPin.ThrownExceptions
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
                    .ToPropertyEx(this, x => x.RequirePin, false)
                    .DisposeWith(d);


                var passErrStream = mayCausedError.Where(u => (u is ConnectionException))
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
                    });

                mayCausedError.Select(u => u.Message)
                    .Merge(this.CancelConnect.Select(_ => "您已取消连接"))
                    .ToPropertyEx(this, x => x.FailMessage)
                    .DisposeWith(d);

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
                    .Merge(passErrStream)
                    .Merge(this.CancelConnect.Select(_ => true))
                    .ToPropertyEx(this, x => x.IsFail, false)
                    .DisposeWith(d);

                this.WhenAnyValue(u => u.Global.ConnectStatus)
                    .Select(u => u == ConnectStatus.Connected)
                    .ToPropertyEx(this, x => x.IsSuccess)
                    .DisposeWith(d);

                this.WhenAnyValue(u => u.Global.ConnectStatus)
                    .ToPropertyEx(this, x => x.Status)
                    .DisposeWith(d);

            });


        }

        private IObservable<Unit> CancelConectImpl()
        {
            return Observable.StartAsync(async () =>
            {
                this.Global.ConnectStatus = ConnectStatus.Disconnected;
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
                var c = this.Global.DoConnect;
                await c.Execute(pin);
            });
        }

        private IObservable<Unit> ConnectImpl()
        {
            return Observable.StartAsync(async () =>
            {
                await this.Global.Connect.Execute();
            });

        }
    }
}
