﻿using NEU.IPGateWay.Core.Models;
using NEU.IPGateWay.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEU.IPGateWay.Core
{
    public class GlobalStatusStore : ReactiveObject
    {
        [Reactive]
        public User CurrentUser { get; set; }

        [Reactive]
        public ConnectStatus ConnectStatus { get; set; }


        readonly ObservableAsPropertyHelper<bool> _canConnect;

        public bool CanOperate => _canConnect.Value;

        /// <summary>
        /// 执行连接，需要订阅异常
        /// </summary>
        public ReactiveCommand<string, bool> DoConnect { get; }

        /// <summary>
        /// 连接，如果需要PIN抛异常，手动调用DoConnect，需要订阅异常
        /// </summary>
        public ReactiveCommand<Unit, Unit> Connect { get; }

        public ReactiveCommand<Unit, bool> Disconnect { get; }

        public ReactiveCommand<Unit, Unit> Toggle { get; }


        #region 工厂方法

        private async Task InitializeStatus()
        {
            var userService = Locator.Current.GetService<IUserStorageService>();
            CurrentUser = await userService.GetDefaultUser();

        }

        private GlobalStatusStore()
        {
            var canOperate = this.WhenAnyValue(x => x.ConnectStatus)
               .Select(p =>
               {
                   if (p == ConnectStatus.Checking &&
                       p == ConnectStatus.Connecting &&
                       p == ConnectStatus.Disconnecting)
                   {
                       return false;
                   }
                   else
                   {
                       return true;
                   }
               });


            _canConnect = canOperate.ToProperty(this, x => x.CanOperate, false);

            Connect = ReactiveCommand.CreateFromTask(async () =>
            {
                ConnectStatus = ConnectStatus.Connecting;

                if (await Locator.Current.GetService<IUserStorageService>().CheckUserPinExist(CurrentUser.Username))
                {
                    throw new ConnectionException(ConnectionError.LostPin);
                }
                else
                {
                    await DoConnect.Execute("");
                }
            }, canOperate);


            DoConnect = ReactiveCommand.CreateFromTask<string, bool>(async (pin) =>
            {
                string password;
                try
                {
                    password = await Locator.Current.GetService<IUserStorageService>().DecryptedUserPassword(CurrentUser.Username, pin);
                }
                catch
                {
                    throw new ConnectionException(ConnectionError.InvalidPin);
                }

                return await Locator.Current.GetService<IInternetGateWayService>().Connect(CurrentUser.Username, password);
                

            }, canOperate);


            DoConnect.Where(u => u).Subscribe(p =>
            {
                ConnectStatus = ConnectStatus.Connected;
            });


            Disconnect = ReactiveCommand.CreateFromTask(async () =>
            {
                ConnectStatus = ConnectStatus.Disconnecting;
                return await Locator.Current.GetService<IInternetGateWayService>().Disconnect();
            }, canOperate);

            Disconnect.Where(u => u).Subscribe(p =>
            {
                ConnectStatus = ConnectStatus.Disconnected;
            });

            Toggle = ReactiveCommand.CreateFromTask(async () =>
            {
                Connect.ThrownExceptions.Subscribe(ex => { });
                if (ConnectStatus == ConnectStatus.Disconnected) await Connect.Execute();
                else if (ConnectStatus == ConnectStatus.Connected) await Disconnect.Execute();

            }, canOperate);

            InitializeStatus();
        }

        public static GlobalStatusStore Current { get; } = new GlobalStatusStore();



        #endregion

    }
}
