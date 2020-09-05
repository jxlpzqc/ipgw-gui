using NEU.IPGateway.UI.Controls;
using NEU.IPGateway.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for RemindConnectPopupWindow.xaml
    /// </summary>
    public partial class RemindConnectPopupWindow : BaseWindow<RemindConnectViewModel>
    {
        public RemindConnectPopupWindow()
        {
            InitializeComponent();

            var screenW = SystemParameters.WorkArea.Width;
            var screenH = SystemParameters.WorkArea.Height;

            this.Left = screenW - this.Width;
            this.Top = screenH - this.Height;

            InitializeViewModel();
        }

        private SolidColorBrush successBrush = new SolidColorBrush(Color.FromRgb(0xA6, 0xDC, 0xA6));
        private SolidColorBrush errorBrush = new SolidColorBrush(Color.FromRgb(0xF0, 0xAE, 0xAE));


        private void InitializeViewModel()
        {
            ViewModel = new RemindConnectViewModel();

            // Binding
            this.WhenActivated((d) =>
            {
                this.OneWayBind(ViewModel,
                    vm => vm.Status,
                    v => v.connectBtn.Status
                    )
                    .DisposeWith(d);

                this.WhenAnyValue(x => x.ViewModel.IsSuccess)
                    .Merge(this.WhenAnyValue(x=>x.ViewModel.IsFail))
                    .Where(u => u)
                    .Subscribe(_ =>
                    {
                        ShowConnectAnimate();
                    })
                    .DisposeWith(d);

                this.WhenAnyValue(x => x.ViewModel.Status)
                    .Where(u => u != IPGateway.Core.Models.ConnectStatus.Disconnected)
                    .Subscribe(_ =>
                    {
                        remindCb.Visibility = Visibility.Hidden;
                    })
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.Connect,
                    v => v.connectBtn)
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    vm => vm.CancelConnect,
                    v => v.cancelBtn)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.RemainCloseSecond,
                    x => x.secondText.Text)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    vm => vm.IsFail,
                    x => x.failP.Visibility)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    vm => vm.IsFail,
                    x => x.overlay.Background,
                    isFail => isFail ? errorBrush: successBrush)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.IsSuccess,
                    x => x.successP.Visibility)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    vm => vm.FailMessage,
                    x => x.errorText.Text)
                    .DisposeWith(d);

                this.Bind(ViewModel,
                    vm => vm.IsNotRemindMeLater,
                    x => x.remindCb.IsChecked)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    vm => vm.RequirePin,
                    x => x.PinVisible)
                    .DisposeWith(d);

                this.WhenAnyValue(u => u.ViewModel.NeedClose)
                    .Where(u => u)
                    .Subscribe((x) =>
                    {
                        Close();
                    })
                    .DisposeWith(d);

            });
        }


        private void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {


        }


        private async void ShowConnectAnimate()
        {
            ((Storyboard)Resources["connectedAnimation"]).Begin();
            await Task.Delay(1000);
        }

        private void pinInput_FinishedInput(object sender, EventArgs e)
        {
            if (sender is PinInput input)
            {
                var pin = input.Pin;
                input.Reset();

                ViewModel.ContinueConnectWithPin.Execute(pin).Catch(Observable.Return(Unit.Default)).Subscribe();

                
            }
        }

        private bool _pinVisible = false;

        private bool PinVisible
        {
            get
            {
                return _pinVisible;
            }
            set
            {
                if (_pinVisible != value)
                {
                    if (value) ShowPinInputAnimate();
                    else HidePinInputAnimate();
                }

                _pinVisible = value;
            }
        }


        private bool _inputShowLock = false;

        private async Task EnsurePinInputAnimationSecurity()
        {
            while (true)
            {
                if (_inputShowLock == false) return;
                else await Task.Delay(300);
            }
        }

        private async Task ShowPinInputAnimate()
        {
            await EnsurePinInputAnimationSecurity();
            _inputShowLock = true;

            var time = 1200;
            var timeSpan = TimeSpan.FromMilliseconds(time);
            var effect = new BlurEffect()
            {
                Radius = 0
            };



            grid.Effect = effect;
            var animation = new DoubleAnimation(30, new Duration(timeSpan));
            var opacityAnimation = new DoubleAnimation(1, new Duration(timeSpan));
            effect.BeginAnimation(BlurEffect.RadiusProperty, animation);
            grid.IsHitTestVisible = false;
            pinInputGrid.Visibility = Visibility.Visible;
            pinInputGrid.BeginAnimation(OpacityProperty, opacityAnimation);
            pinInputGrid.Focus();

            await Task.Delay(timeSpan);

            grid.IsHitTestVisible = false;
            pinInputGrid.Visibility = Visibility.Visible;
            pinInput.Focus();
            _inputShowLock = false;
        }


        private async Task HidePinInputAnimate()
        {

            await EnsurePinInputAnimationSecurity();
            _inputShowLock = true;

            var timeSpan = TimeSpan.FromMilliseconds(1200);

            var animation = new DoubleAnimation(0, new Duration(timeSpan));

            if (grid.Effect != null)
                ((BlurEffect)grid.Effect).BeginAnimation(BlurEffect.RadiusProperty, animation);
            pinInputGrid.BeginAnimation(OpacityProperty, animation);


            await Task.Delay(timeSpan);

            pinInputGrid.Visibility = Visibility.Collapsed;
            grid.IsHitTestVisible = true;
            grid.Effect = null;

            _inputShowLock = false;
        }
    }
}
