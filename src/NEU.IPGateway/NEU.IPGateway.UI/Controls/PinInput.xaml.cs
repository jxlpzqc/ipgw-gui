using DynamicData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Interaction logic for PinInput.xaml
    /// </summary>
    public partial class PinInput : UserControl
    {

        private char[] pin = new char[4];

        public string Pin
        {
            get
            {
                return string.Concat(pin);
            }
        }

        private TextBox[] inputs;

        public static readonly RoutedEvent FinishedInputEvent = EventManager.RegisterRoutedEvent("FinishedInput", RoutingStrategy.Direct, typeof(EventHandler), typeof(PinInput));

        public event EventHandler FinishedInput
        {
            add { AddHandler(FinishedInputEvent, value); }
            remove { RemoveHandler(FinishedInputEvent, value); }
        }

        private void FocusToNext()
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].IsFocused)
                {
                    //inputs[(i + 1) % (inputs.Length)].Focus();
                    inputs[Math.Min(i + 1, inputs.Length - 1)].Focus();
                    return;
                }
            }
        }

        private void FocusToPrevious()
        {
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].IsFocused)
                {
                    inputs[Math.Max(i - 1, 0)].Focus();
                    return;
                }
            }
        }

        public void Reset()
        {
            for (int i = 0; i < pin.Length; i++)
            {
                pin[i] = '\0';
                inputs[i].Text = "";
            }
            inputs[0].Focus();

        }

        public PinInput()
        {

            InitializeComponent();

            inputs = new TextBox[] { input1, input2, input3, input4 };
            foreach (var input in inputs)
            {
                input.SetValue(InputMethod.IsInputMethodEnabledProperty, false);
                input.IsReadOnly = true;
                input.GotFocus += Input_GotFocus;
                input.PreviewKeyDown += Input_KeyDown;
            }

        }

        private void SetPin(TextBox control,int c)
        {
            SetPin(inputs.IndexOf(control), c);
        }

        private void SetPin(int index,int c)
        {
            pin[index] = char.Parse(c.ToString());
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            var control = sender as TextBox;
            if (control == null) return;
            var keyCode = (int)e.Key;
            var passwordStr = "●";

            Action handleInput = () =>
            {
                FocusToNext();
                if (inputs.Last() == control)
                {
                    RaiseEvent(new RoutedEventArgs(FinishedInputEvent, this));
                }
            };

            if (keyCode >= (int)Key.D0 && keyCode <= (int)Key.D9)
            {
                //control.Text = (keyCode - (int)Key.D0).ToString();
                control.Text = passwordStr;
                SetPin(control, (keyCode - (int)Key.D0));
                handleInput();
            }
            else if (keyCode >= (int)Key.NumPad0 && keyCode <= (int)Key.NumPad9)
            {
                //control.Text = (keyCode - (int)Key.NumPad0).ToString();
                control.Text = passwordStr;
                SetPin(control, (keyCode - (int)Key.NumPad0));
                handleInput();
            }
            else if (e.Key == Key.Back)
            {
                control.Text = "";
                inputs[Math.Max(0, inputs.IndexOf(control) - 1)].Text = "";
                FocusToPrevious();
            }
            else if(e.Key == Key.Left)
            {
                FocusToPrevious();
            }
            else if(e.Key == Key.Right)
            {
                FocusToNext();
            }
            e.Handled = true;
        }

        private void Input_GotFocus(object sender, RoutedEventArgs e)
        {
            var control = sender as TextBox;
            if (control == null) return;

            var point = control.TranslatePoint(new Point(-1, -1), canvas);

            var time = 120;
            var newLeft = point.X;
            //设置焦点边框            
            var animate = new DoubleAnimation(newLeft, new Duration(TimeSpan.FromMilliseconds(time)));

            inputFocusBorder.BeginAnimation(Canvas.LeftProperty, animate);

        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            inputFocusBorder.Visibility = Visibility.Visible;
            bool flag = true;
            
            for (int i = 0; i < inputs.Length; i++)
            {
                if (inputs[i].IsFocused) flag = false;
            }

            if (flag) inputs[0].Focus();

        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            inputFocusBorder.Visibility = Visibility.Hidden;
        }
    }
}
