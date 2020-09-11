using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
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
    public class GlobalStatusStore : ReactiveObject
    {
        [Reactive]
        public User CurrentUser { get; set; }

        [Reactive]
        public ConnectStatus ConnectStatus { get; set; }

        [Reactive]
        public Setting Setting { get; set; }

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

        public ReactiveCommand<Unit, Unit> Test { get; }


        public async Task Initialize()
        {
            var settingService = Locator.Current.GetService<ISettingStorageService>();
            try
            {
                Setting = await settingService.ReadSetting();
            }
            catch
            {
                Setting = new Setting();
                await settingService.SaveSetting(Setting);
            }

            try
            {
                var userService = Locator.Current.GetService<IUserStorageService>();
                CurrentUser = await userService.GetDefaultUser();
            }
            catch
            {
                throw new Exception("vm_no_user");
            }
            
        }


        #region 工厂方法

        private GlobalStatusStore()
        {

            Test = ReactiveCommand.CreateFromTask(TestImpl);

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


            Connect.ThrownExceptions.Subscribe(ex => { });


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

                return await Locator.Current.GetService<IInternetGatewayService>().Connect(CurrentUser.Username, password);
                

            }, canOperate);

            DoConnect.ThrownExceptions.Subscribe(ex => { });

            DoConnect.Where(u => u).Subscribe(p =>
            {
                ConnectStatus = ConnectStatus.Connected;
            });


            Disconnect = ReactiveCommand.CreateFromTask(async () =>
            {
                ConnectStatus = ConnectStatus.Disconnecting;
                return await Locator.Current.GetService<IInternetGatewayService>().Disconnect();
            }, canOperate);

            Disconnect.Where(u => u).Subscribe(_ =>
            {
                ConnectStatus = ConnectStatus.Disconnected;
            });

            Disconnect.ThrownExceptions.Subscribe(ex => { });

            Toggle = ReactiveCommand.CreateFromTask(async () =>
            {
                if (ConnectStatus == ConnectStatus.Disconnected) await Connect.Execute();
                else if (ConnectStatus == ConnectStatus.Connected) await Disconnect.Execute();

            }, canOperate);

            Toggle.ThrownExceptions.Subscribe(_ => { });

        }

        private async Task TestImpl()
        {
            var oldStatus = ConnectStatus;
            ConnectStatus = ConnectStatus.Checking;
            try
            {
                var result = await Locator.Current.GetService<IInternetGatewayService>().Test();
                if (!result.connected) ConnectStatus = ConnectStatus.DisconnectedFromNetwork;
                else if (result.logedin) ConnectStatus = ConnectStatus.Connected;
                else ConnectStatus = ConnectStatus.Disconnected;
            }
            catch
            {
                ConnectStatus = oldStatus;
            }
        }

        public static GlobalStatusStore Current { get; } = new GlobalStatusStore();



        #endregion

    }
}
