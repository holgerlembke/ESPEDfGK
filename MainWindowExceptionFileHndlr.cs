
using System.IO;
using System;
using System.Windows;
using System.Windows.Media;

namespace ESPEDfGK
{
    //*****************************************************************************************
    public partial class MainWindow : Window
    {

        //*****************************************************************************************
        void EndExceptionFileMonitor()
        {
            if (exceptionfilemonitor != null)
            {
                // it kills itself
                exceptionfilemonitor.Die = true;
                exceptionfilemonitor = null;
            }
        }

        //*****************************************************************************************
        void StartExceptionFileMonitor()
        {
            EndExceptionFileMonitor();

            ExceptionScanner.ExceptionScanner ex = new();
            exceptionfilemonitor = new(ex.ExceptionFilename);
            exceptionfilemonitor.ExceptionFileFound += ExceptionFileFound;
            exceptionfilemonitor.start();
        }

        Brush oldcolor;

        //*****************************************************************************************
        private void ExceptionFileFound(object sender, EventArgs e)
        {
            BtCopyFromSerialMonitorFile.IsEnabled = true;
            BtCopyFromSerialMonitorFile.Background = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        }

        //*****************************************************************************************
        private void BtCopyFromSerialMonitorFileClick(object sender, RoutedEventArgs e)
        {
            BtCopyFromSerialMonitorFile.Background = oldcolor;
            string content = File.ReadAllText(exceptionfilemonitor.FileMonitored);
            TBStackdump.Text = content;
        }

    }
}
