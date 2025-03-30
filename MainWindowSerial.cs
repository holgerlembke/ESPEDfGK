
using System;
using System.IO.Ports;
using System.Text;
using System.Windows;
using System.Windows.Media;
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
        ExceptionScanner.ExceptionScanner exceptionscanner = new();
        string serprotocol = "";
        const int serprotocolmax = 5 * 1024;
        string exceptionmsg = "";

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
                exceptionmsg = "";
                BtnCopyExceptionToAnalyzer.Background = oldcolor;
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
            string recievedData = serialPort.ReadExisting();

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

            // Exception-Scanner
            if (serprotocol.Length > serprotocolmax)
            {
                serprotocol = serprotocol.Remove(0, serprotocol.Length - serprotocolmax);
            }
            serprotocol += text;

            if (exceptionmsg == "")
            {
                exceptionmsg = exceptionscanner.checkAndSeparateExceptionText(serprotocol);
                if (exceptionmsg != "")
                {
                    BtnCopyExceptionToAnalyzer.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                }
            }

            tBoxInData.Text += text;
        }
        //*****************************************************************************************
        private void tBoxOutDataKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string[] add = new string[4] { "", "\n", "\r", "\r\n" };

            if ((e.Key == System.Windows.Input.Key.Enter) &&
                (serialPort.IsOpen) &&
                (tBoxOutData.Text != ""))
            {
                //UTF8?
                byte[] buffer = Encoding.ASCII.GetBytes(tBoxOutData.Text +
                                                        add[cBoxOutDataAdd.SelectedIndex]);
                try
                {
                    serialPort.Write(buffer, 0, buffer.Length);
                }
                catch (Exception)
                {
                    // eat it.
                }
                tBoxOutData.Text = "";
                e.Handled = true;
            }
        }
        //*****************************************************************************************
        private void BtnClearSermonitor(object sender, RoutedEventArgs e)
        {
            exceptionmsg = "";
            serprotocol = "";
            tBoxInData.Text = "";
            BtnCopyExceptionToAnalyzer.Background = oldcolor;
        }
        //*****************************************************************************************
        private void BtnCopyToAnalyzer(object sender, RoutedEventArgs e)
        {
            if (tBoxInData.Text != "")
            {
                TBStackdump.Text = tBoxInData.Text;
            }
        }
        //*****************************************************************************************
        private void BtnCopyToClipboard(object sender, RoutedEventArgs e)
        {
            if (tBoxInData.Text != "")
            {
                Clipboard.SetText(tBoxInData.Text);
            }
        }
        //*****************************************************************************************
        private void BtnCopyExceptionToAnalyzerClick(object sender, RoutedEventArgs e)
        {
            if (exceptionmsg != "")
            {
                TBStackdump.Text = exceptionmsg;
                BtnCopyExceptionToAnalyzer.Background = oldcolor;
                serprotocol = "";
                exceptionmsg = "";
            }
        }
        //*****************************************************************************************
        private void cBoxDropDownOpened(object sender, EventArgs e)
        {
            string s = cBoxComPort.Text;
            cBoxComPort.Items.Clear();
            RefreshPortList();
            cBoxComPort.Text = s;
        }
    }
}
