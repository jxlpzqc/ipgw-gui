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
    public class WelcomeViewModel : ReactiveObject
    {
        public enum Step
        {
            SelectLanguage,
            SetAccount,
            SetPin,
            SetSundry,
            Welcome
        }

        [Reactive]
        public Step CurrentStep { get; set; }

        [Reactive]
        public bool CanBack { get; set; }

        [Reactive]
        public bool CanContinue { get; set; }

        [Reactive]
        public string Username { get; set; }

        [Reactive]
        public string Password { get; set; }

        [Reactive]
        public string Pin { get; set; }

        [Reactive]
        public string Language { get; set; } = "zh-cn";

        [Reactive]
        public bool LaunchWhenStartup { get; set; } = true;

        [Reactive]
        public bool RemindConnect { get; set; } = true;

        [Reactive]
        public bool AutoConnect { get; set; } = false;

        public ReactiveCommand<Unit, Unit> FinishEnterPin { get; }

        public ReactiveCommand<Unit, Unit> SkipEnterPin { get; }

        public ReactiveCommand<Unit, Unit> Continue { get; }

        public ReactiveCommand<Unit, Unit> GoBack { get; }

        public WelcomeViewModel()
        {

            this.FinishEnterPin = ReactiveCommand.CreateFromTask(FinishEnterPinImpl,
                this.WhenAnyValue(x => x.Pin).Select(x => x?.Length == 4));

            this.SkipEnterPin = ReactiveCommand.CreateFromTask(SkipEnterPinImpl);

            this.Continue = ReactiveCommand.CreateFromTask(ContinueImpl, this.WhenAnyValue(x => x.CanContinue));

            this.GoBack = ReactiveCommand.CreateFromTask(GoBackImpl, this.WhenAnyValue(x => x.CanBack));

            this.WhenAnyValue(x => x.Language)
                .Subscribe(x =>
                {
                    GlobalStatusStore.Current.Setting.Language = x;
                });

            Observable.Merge(
                this.WhenAnyValue(x => x.CurrentStep).Select(u => Unit.Default),
                this.WhenAnyValue(x => x.Username).Select(u => Unit.Default),
                this.WhenAnyValue(x => x.Password).Select(u => Unit.Default),
                this.WhenAnyValue(x => x.Pin).Select(u => Unit.Default)
            ).Subscribe(_ =>
            {
                DeterminePageChangable();
            });

        }

        private void DeterminePageChangable()
        {
            if (CurrentStep == Step.SetAccount)
            {
                CanBack = true;
                if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                    CanContinue = false;
                else
                    CanContinue = true;
            }
            else if (CurrentStep == Step.SetPin)
            {
                CanBack = true;
                if (Pin?.Length == 4) CanContinue = true;
                else CanContinue = false;
            }
            else if (CurrentStep == Step.SetSundry || CurrentStep == Step.SelectLanguage)
            {
                CanBack = false;
                CanContinue = true;
            }
            else
            {
                CanBack = true;
                CanContinue = true;
            }
        }


        private async Task ContinueImpl()
        {
            if(CurrentStep == Step.SetSundry)
            {
                GlobalStatusStore.Current.Setting.LaunchWhenStartup = LaunchWhenStartup;
                GlobalStatusStore.Current.Setting.RemindConnect = RemindConnect;
                GlobalStatusStore.Current.Setting.AutoConnect = AutoConnect;
            }
            if(CurrentStep == Step.SetPin)
            {
                await FinishEnterPinImpl();
            }

            CurrentStep = (Step)((int)CurrentStep + 1);
            
        }

        private async Task GoBackImpl()
        {
            CurrentStep = (Step)((int)CurrentStep - 1);
        }



        private async Task SkipEnterPinImpl()
        {
            var service = Locator.Current.GetService<Services.IUserStorageService>();
            await service.SaveUser(Username, Password, "");
            CurrentStep = (Step)((int)CurrentStep + 1);
        }

        private async Task FinishEnterPinImpl()
        {
            var service = Locator.Current.GetService<Services.IUserStorageService>();
            await service.SaveUser(Username, Password, Pin);
        }
    }
}
