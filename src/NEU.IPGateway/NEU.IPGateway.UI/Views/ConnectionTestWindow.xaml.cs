﻿using NEU.IPGateway.UI.Languages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for ConnectionTestWindow.xaml
    /// </summary>
    public partial class ConnectionTestWindow : ReactiveWindow<Core.ConnectionTestViewModel>
    {
        public ConnectionTestWindow()
        {
            InitializeComponent();

            ViewModel = new Core.ConnectionTestViewModel();

            var green = new SolidColorBrush(Colors.DarkGreen);
            var red = new SolidColorBrush(Colors.DarkRed);

            this.WhenActivated(d =>
            {
                this.OneWayBind(ViewModel,
                    x => x.IsRunning,
                    v => v.runningGrid.Visibility)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.IsRunning,
                    v => v.resultGrid.Visibility,
                    x => x ? Visibility.Collapsed : Visibility.Visible)
                    .DisposeWith(d);


                this.OneWayBind(ViewModel,
                    x => x.IsConnected,
                    v => v.connectedText.Text,
                    connected => connected ? I18NStringUtil.GetString("ct_connected") : I18NStringUtil.GetString("ct_not_connected"))
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.IsConnected,
                    v => v.connectedText.Foreground,
                    connected => connected ? green : red)
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                    x => x.IsLogined,
                    v => v.loginedText.Text,
                    connected => connected ? I18NStringUtil.GetString("ct_logined") : I18NStringUtil.GetString("ct_not_logined"))
                    .DisposeWith(d);

                this.OneWayBind(ViewModel,
                   x => x.IsLogined,
                   v => v.loginedText.Foreground,
                   connected => connected ? green : red)
                   .DisposeWith(d);


                this.BindCommand(ViewModel,
                    x => x.Test,
                    v => v.retestBtn)
                    .DisposeWith(d);

            });

        }
    }
}
