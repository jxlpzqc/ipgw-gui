using NEU.IPGateWay.Core;
using ReactiveUI;
using System;
using System.Collections.Generic;
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

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for RemindConnectPopupWindow.xaml
    /// </summary>
    public partial class RemindConnectPopupWindow : BaseWindow<MainPageViewModel>
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

        private void InitializeViewModel()
        {
            ViewModel = new MainPageViewModel();

            this.WhenActivated((d) =>
            {


            });
        }


        private  void BaseWindow_Loaded(object sender, RoutedEventArgs e)
        {


        }


        private async void ShowConnectAnimate()
        {
            ((Storyboard)Resources["connectedAnimation"]).Begin();
            await Task.Delay(1000);
        } 
    }
}
