﻿using NEU.IPGateWay.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Controls
{
    /// <summary>
    /// Interaction logic for ConnectButton.xaml
    /// </summary>
    public partial class ConnectButton : Button
    {
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(ConnectStatus), typeof(ConnectButton), new PropertyMetadata(ConnectStatus.Disconnected, OnStatusChanged));

        public ConnectStatus Status
        {
            get => (ConnectStatus)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (ConnectButton)d;
            var value = (ConnectStatus)e.NewValue;


            var connectLogoAnimation = ((Storyboard)self.Resources["ConnectingLogoAnimation"]);
            var runningAnimation = ((Storyboard)self.Resources["RunningAnimation"]);

            connectLogoAnimation.Stop();
            runningAnimation.Stop();


            Action showConnectButton = () =>
            {
                self.disconnect.Visibility = Visibility.Hidden;
                self.connect.Visibility = Visibility.Visible;
            };

            Action showDisconnectButton = () =>
            {

                self.disconnect.Visibility = Visibility.Visible;
                self.connect.Visibility = Visibility.Hidden;
            };


            if (value == ConnectStatus.Disconnected)
            {
                showConnectButton();
            }
            else if (value == ConnectStatus.Connecting)
            {
                showConnectButton();
                connectLogoAnimation.Begin();
                runningAnimation.Begin();
            }
            else if (value == ConnectStatus.Disconnecting)
            {
                showDisconnectButton();
                runningAnimation.Begin();
            }
            else if(value == ConnectStatus.Connected)
            {
                showDisconnectButton();
            }

        }

        public ConnectButton()
        {

            InitializeComponent();
        }
    }

    
}
