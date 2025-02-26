
using System;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;

namespace ESPEDfGK
{
    public enum ShortParity
    {
        N = 0,
        O = 1,
        E = 2,
        M = 3,
        S = 4,
    }

    //*****************************************************************************************
    public partial class MainWindow : Window
    {
        // lots of this is cut/paste from https://github.com/minhncedutw/wpf-serial-communication

        SerialPort serialPort = new SerialPort();
        string recievedData;

        //*****************************************************************************************
        private void BtnOpenSermonitor(object sender, RoutedEventArgs e)
        {
            try
            {
                serialPort.PortName = cBoxComPort.Text;
                serialPort.BaudRate = Convert.ToInt32(cBoxBaudRate.Text);
                serialPort.DataBits = Convert.ToInt32(cBoxDataBits.Text);
                serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), cBoxStopBits.Text);
                serialPort.Parity = (Parity)Enum.Parse(typeof(ShortParity), cBoxParityBits.Text);
                serialPort.Open(); // Open port.
                serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataRecieved);
                btnOpen.Visibility = Visibility.Collapsed;
                btnClose.Visibility = Visibility.Visible;
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //*****************************************************************************************
        private void BtnCloseSermonitor(object sender, RoutedEventArgs e)
        {
            btnOpen.Visibility = Visibility.Visible;
            btnClose.Visibility = Visibility.Collapsed;
            serialPort.Close();
        }

        //*****************************************************************************************
        private delegate void UpdateUiTextDelegate(string text);
        //*****************************************************************************************
        private void serialPort_DataRecieved(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            // Collecting the characters received to our 'buffer' (string).
            recievedData = serialPort.ReadExisting();

            // Delegate a function to display the received data.
            Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(DataWrited), recievedData);
        }
        //*****************************************************************************************
        private void DataWrited(string text)
        {
            // Limit text
            while (tBoxInData.Text.Length > 50 * 1024)
            {
                tBoxInData.Text = tBoxInData.Text.Remove(0, 1024);
            }

            tBoxInData.Text += text;
        }
        //*****************************************************************************************
        private void BtnClearSermonitor(object sender, RoutedEventArgs e)
        {
            tBoxInData.Text = "";
        }
        //*****************************************************************************************
        private void BtnCopyToAnalyzer(object sender, RoutedEventArgs e)
        {
            TBStackdump.Text = tBoxInData.Text;
        }
        //*****************************************************************************************
        private void BtnCopyToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tBoxInData.Text);
        }
    }
}
