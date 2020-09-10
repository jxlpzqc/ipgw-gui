using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace NEU.IPGateway.Core
{
    public class UserViewModel : ReactiveObject,IActivatableViewModel
    {

        [ObservableAsProperty]
        public bool IsPasswordShown { get; }

        [ObservableAsProperty]
        public bool HasPin { get; }

        [ObservableAsProperty]
        public bool IsCurrent { get; }

        [Reactive]
        public User User { get; set; }

        public ReactiveCommand<Unit, bool> Delete { get; }

        public ReactiveCommand<Unit,Unit> Refresh { get; }

        public ReactiveCommand<(string password,string pin), bool> ChangePassword { get; }

        public ReactiveCommand<(string oldPin, string newPin), bool> ChangePin { get; }

        public ReactiveCommand<string, bool> TogglePasswordShown { get; }

        public ReactiveCommand<Unit, Unit> HidePassword { get; }

        public ReactiveCommand<string, bool> VerifyPin { get; }

        public ReactiveCommand<Unit, Unit> SetCurrent { get; }

        public ViewModelActivator Activator { get; }

        
        public UserViewModel()
        {
            Activator = new ViewModelActivator();

            IUserStorageService service = Locator.Current.GetService<IUserStorageService>();

            #region Commands
            Refresh = ReactiveCommand.CreateFromObservable(() => Observable.Return(Unit.Default));

            Delete = ReactiveCommand.CreateFromTask(async () =>
            {
                var ret = await service.DeleteUser(User.Username);
                await Refresh.Execute();
                return ret;
            });

            ChangePassword = ReactiveCommand.CreateFromTask<(string password, string pin), bool>(async (input) =>
            {
                var ret = await service.ResetUserPassword(User.Username, input.password, input.pin);
                await Refresh.Execute();
                return ret;
            });

            ChangePin = ReactiveCommand.CreateFromTask<(string oldPin, string newPin), bool>(async (input) =>
            {
                var ret = await service.ResetUserPin(User.Username, input.oldPin, input.newPin);
                await Refresh.Execute();
                return ret;
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

            SetCurrent = ReactiveCommand.CreateFromTask(async () =>
            {
                var status = GlobalStatusStore.Current.ConnectStatus;
                if (status != ConnectStatus.Disconnected) throw new Exception("vm_unavailable_change_user");

                GlobalStatusStore.Current.CurrentUser = User;
                await service.SetDefaultUser(User.Username);
            });

            #endregion

            this.WhenActivated(d =>
            {

                this.WhenAnyValue(x => x.User).SelectMany(x => Observable.FromAsync(async () =>
                {
                    return await service.CheckUserPinExist(x?.Username);
                }))
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.HasPin)
                    .DisposeWith(d);

                this.WhenAnyValue(x => x.User)
                    .CombineLatest(
                        GlobalStatusStore.Current.WhenAnyValue(x => x.CurrentUser),
                        (x, y) => (x?.Username == y?.Username)
                    )
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .ToPropertyEx(this, x => x.IsCurrent)
                    .DisposeWith(d);

            });


        }
        
    }
}
