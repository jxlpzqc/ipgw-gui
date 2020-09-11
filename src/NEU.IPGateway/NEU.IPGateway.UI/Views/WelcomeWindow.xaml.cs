using NEU.IPGateway.UI.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static NEU.IPGateway.Core.WelcomeViewModel;

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for WelcomeWindow.xaml
    /// </summary>
    public partial class WelcomeWindow : BaseWindow<Core.WelcomeViewModel>
    {
        public WelcomeWindow()
        {
            InitializeComponent();
            this.ViewModel = new Core.WelcomeViewModel();
            SetAllTransparent();


            this.WhenActivated(d =>
            {
                this.Bind(ViewModel,
                    x => x.Language,
                    v => v.languageLb.SelectedIndex,
                    vmToViewConverter: x => x == "en-us" ? 1 : 0,
                    viewToVmConverter: x => x == 1 ? "en-us" : "zh-cn")
                    .DisposeWith(d);

                this.BindCommand(ViewModel,
                    x => x.Continue, v => v.continueBtn)
                    .DisposeWith(d);


                this.BindCommand(ViewModel,
                    x => x.SkipEnterPin, v => v.skipBtn)
                    .DisposeWith(d);



                this.BindCommand(ViewModel,
                    x => x.GoBack, v => v.backBtn)
                    .DisposeWith(d);


                this.Bind(ViewModel,
                    x => x.RemindConnect,
                    v => v.remindCb.IsChecked)
                    .DisposeWith(d);



                this.Bind(ViewModel,
                    x => x.AutoConnect,
                    v => v.autoCb.IsChecked)
                    .DisposeWith(d);


                this.Bind(ViewModel,
                    x => x.LaunchWhenStartup,
                    v => v.launchCb.IsChecked)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    x => x.Username,
                    v => v.usernameLb.Content,
                    x => string.IsNullOrEmpty(x) ? " ----- " : x)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    x => x.Password,
                    v => v.passwordLb.Content,
                    x => string.IsNullOrEmpty(x) ? " ----- " : x)
                    .DisposeWith(d);



                this.WhenAnyValue(x => x.ViewModel.CurrentStep)
                    .Subscribe(async x => await ChangePage(x))
                    .DisposeWith(d);


            });
        }

        private void SetAllTransparent()
        {
            foreach (UIElement item in grid.Children)
            {
                item.Opacity = 0;
                item.Visibility = Visibility.Collapsed;
            }
        }

        private Step currentStep = Step.SelectLanguage;

        private async Task ChangePage(Step step)
        {

            var timeSpan = TimeSpan.FromMilliseconds(1200);
            var timeSpan2 = TimeSpan.FromMilliseconds(600);
            var duration = new Duration(timeSpan);
            var duration2 = new Duration(timeSpan2);
            var stepInt = (int)step;
            var toS = -2 + 2 * stepInt / 4;
            var toE = 1 + 2 * stepInt / 4;
            var animationS = new PointAnimation(new Point(0.5, toS), duration);
            var animationE = new PointAnimation(new Point(0.5, toE), duration);
            winBg.BeginAnimation(LinearGradientBrush.StartPointProperty, animationS);
            winBg.BeginAnimation(LinearGradientBrush.EndPointProperty, animationE);

            var disappearAnimation = new DoubleAnimation(0, duration2);
            var appearAnimation = new DoubleAnimation(1, duration2);

            grid.Children[(int)currentStep].BeginAnimation(UIElement.OpacityProperty, disappearAnimation);
            await Task.Delay(timeSpan2);

            grid.Children[(int)currentStep].Visibility = Visibility.Collapsed;
            grid.Children[stepInt].Visibility = Visibility.Visible;

            grid.Children[stepInt].BeginAnimation(UIElement.OpacityProperty, appearAnimation);
            await Task.Delay(timeSpan2);

            currentStep = step;

            if (currentStep == Step.SelectLanguage) languageLb.Focus();
            if (currentStep == Step.Welcome) handleBar.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoginWindow();
            if (dialog.ShowDialog() == true)
            {
                ViewModel.Username = dialog.Result.Username;
                ViewModel.Password = dialog.Result.Password;
            }
        }

        private void PinInput_FinishedInput(object sender, EventArgs e)
        {
            ViewModel.Pin = ((PinInput)sender).Pin;
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;

        }
    }
}
