using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Views
{
    /// <summary>
    /// Interaction logic for BaseWindow.xaml
    /// </summary>
    public abstract class BaseWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
    {
        public BaseWindow()
        {
            Style = (Style)Application.Current.Resources["BaseWindowStyle"];


            Loaded += (e, s) =>
            {
                ControlTemplate baseWindowTemplate = this.Template;

                var btn = ((Button)baseWindowTemplate.FindName("BaseWindowCloseButton", this));

                btn.Click += CloseWindowButton_Click;

                var border = ((Border)baseWindowTemplate.FindName("BaseWindowBorder", this));

                border.MouseDown += Border_MouseDown;
            };


        }



        public override void EndInit()
        {
            base.EndInit();

        }


        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ResizeMode = ResizeMode.NoResize;
                DragMove();
            }

            if (e.LeftButton == MouseButtonState.Released)
            {
                ResizeMode = ResizeMode.CanResize;
            }
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.WndProc));
            }
        }


        private const int WM_NCHITTEST = 0x0084;
        private const int WM_GETMINMAXINFO = 0x0024;
        private readonly int agWidth = 12; //拐角宽度
        private readonly int bThickness = 12; // 边框宽度



        public enum HitTest : int
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTSIZE = HTGROWBOX,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTREDUCE = HTMINBUTTON,
            HTZOOM = HTMAXBUTTON,
            HTSIZEFIRST = HTLEFT,
            HTSIZELAST = HTBOTTOMRIGHT,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21,
        }

        protected virtual IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_NCHITTEST:
                    var screenX = (lParam.ToInt32() & 0xFFFF);
                    var screenY = (lParam.ToInt32() >> 16);

                    //测试鼠标位置

                    var point = this.PointFromScreen(new Point(screenX, screenY));

                    // 窗口左上角
                    if (point.X <= this.agWidth
                       && point.Y <= this.agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTTOPLEFT);
                    }
                    // 窗口左下角    
                    else if (this.ActualHeight - point.Y <= this.agWidth
                       && point.X <= this.agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTBOTTOMLEFT);
                    }
                    // 窗口右上角
                    else if (point.Y <= this.agWidth
                       && this.ActualWidth - point.X <= this.agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTTOPRIGHT);
                    }
                    // 窗口右下角
                    else if (this.ActualWidth - point.X <= this.agWidth
                       && this.ActualHeight - point.Y <= this.agWidth)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTBOTTOMRIGHT);
                    }
                    // 窗口左侧
                    else if (point.X <= this.bThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTLEFT);
                    }
                    // 窗口右侧
                    else if (this.ActualWidth - point.X <= this.bThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTRIGHT);
                    }
                    // 窗口上方
                    else if (point.Y <= this.bThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTTOP);
                    }
                    // 窗口下方
                    else if (this.ActualHeight - point.Y <= this.bThickness)
                    {
                        handled = true;
                        return new IntPtr((int)HitTest.HTBOTTOM);
                    }
                    else
                    {
                        return IntPtr.Zero;
                    }


            }
            return IntPtr.Zero;
        }

    }


    class BaseWindowMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value - 20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value + 20;
        }
    }

}
