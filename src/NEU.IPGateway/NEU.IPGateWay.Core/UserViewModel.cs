using NEU.IPGateWay.Core.Models;
using NEU.IPGateWay.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace NEU.IPGateWay.Core
{
    public class UserViewModel : ReactiveObject,IDisposable
    {

        public extern bool IsPasswordShown
        {
            [ObservableAsProperty]
            get;
        }

        public extern bool HasPin
        {
            [ObservableAsProperty]
            get;
        }

        public extern bool IsCurrent
        {
            [ObservableAsProperty]
            get;
        }



        [Reactive]
        public User User { get; set; }

        /// <summary>
        /// 删除命令，如果属于一个列表，删除成功后应该由列表所有订阅并从列表移除
        /// </summary>
        public ReactiveCommand<Unit, bool> Delete { get; }

        public ReactiveCommand<string, bool> ChangePassword { get; }

        public ReactiveCommand<(string oldPin, string newPin), bool> ChangePin { get; }

        public ReactiveCommand<string, bool> TogglePasswordShown { get; }

        public ReactiveCommand<Unit, Unit> HidePassword { get; }

        public ReactiveCommand<string, bool> VerifyPin { get; }

        public ReactiveCommand<Unit, Unit> SetCurrent { get; }

        private CompositeDisposable disposables = new CompositeDisposable();

        public UserViewModel()
        {
            IUserStorageService service = Locator.Current.GetService<IUserStorageService>();

            this.WhenAnyValue(x => x.User).SelectMany(x => Observable.FromAsync(async () =>
            {
                return await service.CheckUserPinExist(x?.Username);
            })).ToPropertyEx(this, x => x.HasPin);

            this.WhenAnyValue(x => x.User)
                .CombineLatest(
                    GlobalStatusStore.Current.WhenAnyValue(x => x.CurrentUser),
                    (x, y) => (x?.Username == y?.Username)
                )
                .ToPropertyEx(this, x => x.IsCurrent)
                .DisposeWith(disposables);


            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                return await service.DeleteUser(User.Username);
            });

            ChangePassword = ReactiveCommand.CreateFromTask<string, bool>(async (str) =>
            {
                return await service.ResetUserPassword(User.Username, str);
            });

            ChangePin = ReactiveCommand.CreateFromTask<(string oldPin, string newPin), bool>(async (input) =>
            {
                return await service.ResetUserPin(User.Username, input.oldPin, input.newPin);
            });

            VerifyPin = ReactiveCommand.CreateFromTask<string, bool>(async (pin) =>
            {
                return await service.CheckUserPinValid(User.Username, pin);
            });

            TogglePasswordShown = ReactiveCommand.CreateFromTask<string, bool>(async (pin) =>
            {
                if (!IsPasswordShown)
                {
                    try
                    {
                        var password = await service.DecryptedUserPassword(User.Username, pin);
                        User.Password = password;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    User.Password = "";
                    return false;
                }
            });


            TogglePasswordShown.ToPropertyEx(this, x => x.IsPasswordShown, false);

            SetCurrent = ReactiveCommand.Create(() =>
            {
                var status = GlobalStatusStore.Current.ConnectStatus;
                if (status != ConnectStatus.Disconnected) throw new Exception("请先断开连接再切换账户");

                GlobalStatusStore.Current.CurrentUser = User;
            });


        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
