using DynamicData;
using NEU.IPGateway.Core.Models;
using NEU.IPGateway.Core.Services;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace NEU.IPGateway.Core
{
    public class UsersManagerViewModel : ReactiveObject
    {

        //[Reactive]
        //public IEnumerable<UserViewModel> UserViewModels { get; set; }
        public extern IEnumerable<UserViewModel> UserViewModels { [ObservableAsProperty] get; }

        public ReactiveCommand<(User user, string pin), bool> AddUser { get; }

        public ReactiveCommand<Unit, Unit> Refresh { get; } = ReactiveCommand.Create(() => { });

        public UsersManagerViewModel()
        {
            IUserStorageService service = Locator.Current.GetService<IUserStorageService>();
            Observable.Return(Unit.Default)
                .Merge(Refresh)
                .SelectMany(async (e) =>
                {
                    var v = (await service.GetUsers()).Select(u => new UserViewModel
                    {
                        User = u
                    });

                    return v;
                })
                .ObserveOn(RxApp.MainThreadScheduler)
                .ToPropertyEx(this, x => x.UserViewModels);


            // TODO solve the problem
            this.WhenAnyValue(x => x.UserViewModels)
                .Where(u => u != null)
                .Subscribe(x =>
                {
                    foreach (var item in x)
                    {
                        item.Refresh.Subscribe(_ =>
                        {
                            Refresh.Execute().Subscribe();
                        });
                    }

                });



            AddUser = ReactiveCommand.CreateFromTask<(User, string), bool>(async (input) =>
            {
                var user = input.Item1;
                return await service.SaveUser(user.Username, user.Password, input.Item2);
            });

            AddUser.ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ =>
                {
                    Refresh.Execute().Subscribe();
                });

        }


    }
}
