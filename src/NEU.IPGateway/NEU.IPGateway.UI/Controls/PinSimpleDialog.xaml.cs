using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NEU.IPGateway.UI.Controls
{
    /// <summary>
    /// Interaction logic for PinSimpleDialog.xaml
    /// </summary>
    public partial class PinSimpleDialog : Window
    {
        public PinSimpleDialog()
        {
            InitializeComponent();
        }
        
        public PinSimpleDialog(string message)
        {
            InitializeComponent();            
            Message = message;
        }

        public string Message
        {
            get => messageText.Text;
            set => messageText.Text = value;
        }

        public string Result
        {
            get;
            private set;
        }

        private void PinInput_FinishedInput(object sender, EventArgs e)
        {
            var control = (PinInput)sender;
            Result = control.Pin;
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pinInput.Focus();
        }
    }
}
