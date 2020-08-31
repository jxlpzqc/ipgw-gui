using NEU.IPGateWay.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow<MainPageViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainPageViewModel();

            this.WhenActivated((d) =>
            {
                this.OneWayBind(ViewModel,
                    u => u.SelectedUser,
                    v => v.selectedUserLabel.Content,
                    x => x?.Username)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    u => u.ConnectStatus,
                    v => v.connectButton.Status)
                    .DisposeWith(d);

                this.WhenAnyValue(u => u.ViewModel.ConnectStatus)
                    .Subscribe(async p =>
                    {
                        if (p == IPGateWay.Core.Models.ConnectStatus.Connected)
                        {
                            await ShowInformationAnimate();
                        }
                        else
                        {
                            await HideInformationAnimate();
                        }
                    })
                    .DisposeWith(d);



                this.BindCommand(ViewModel, 
                    x => x.Toggle, 
                    v => v.connectButton)
                    .DisposeWith(d);

                ViewModel.DisposeWith(d);

            });

                
        }

        private async Task ShowPinInputAnimate()
        {

            //await Task.Delay(2000);
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


            await Task.Delay(timeSpan);


        }


        private async Task HidePinInputAnimate()
        {
            var timeSpan = TimeSpan.FromMilliseconds(1200);            

            var animation = new DoubleAnimation(0, new Duration(timeSpan));

            ((BlurEffect)grid.Effect).BeginAnimation(BlurEffect.RadiusProperty, animation);
            pinInputGrid.BeginAnimation(OpacityProperty, animation);


            await Task.Delay(timeSpan);

            pinInputGrid.Visibility = Visibility.Collapsed;
            grid.IsHitTestVisible = true;
            grid.Effect = null;
        }

        private async Task ShowInformationAnimate()
        {
            var timeSpan = TimeSpan.FromMilliseconds(1600);
            var animation = new GridLengthStarAnimation()
            {
                To = new GridLength(3,GridUnitType.Star),
                Duration = new Duration(timeSpan),
            };
            grid.ColumnDefinitions[1].BeginAnimation(ColumnDefinition.WidthProperty, animation);
            await Task.Delay(timeSpan);
        }



        private async Task HideInformationAnimate()
        {
            var timeSpan = TimeSpan.FromMilliseconds(1200);
            var animation = new GridLengthStarAnimation()
            {
                To = new GridLength(0, GridUnitType.Star),
                Duration = new Duration(timeSpan),
            };
            grid.ColumnDefinitions[1].BeginAnimation(ColumnDefinition.WidthProperty, animation);
            await Task.Delay(timeSpan);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void settingNavigationBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (Window item in App.Current.Windows)
            {
                if(item is SettingsWindow)
                {
                    item.Focus();
                    return;
                }
            }
            new SettingsWindow().Show();
        }

        private void manageUserBtn_Click(object sender, RoutedEventArgs e)
        {
            new UsersManagerWindow().ShowDialog();
        }
    }
}
