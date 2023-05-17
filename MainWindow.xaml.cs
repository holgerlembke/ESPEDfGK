using Microsoft.Win32;
using System.Configuration;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

namespace ESPEDfGK
{
    //*****************************************************************************************
    public partial class MainWindow : Window
    {
        Konfiguration konfiguration = null;
        KonfigurationsHlpr hlpr = new();
        Addr2Line a2l = new();

        //*****************************************************************************************
        public MainWindow()
        {
            InitializeComponent();

            konfiguration = hlpr.LadeEinstellungen();
            new ExceptionLogger(konfiguration.LoggerDateiname());


            uiScaler.ScaleX = konfiguration.Ink(konfiguration.UIScale, 1.0);
            uiScaler.ScaleY = konfiguration.Ink(konfiguration.UIScale, 1.0);

            Top = konfiguration.Ink(konfiguration.Top, Top);
            Left = konfiguration.Ink(konfiguration.Left, Left);
            Height = konfiguration.Ink(konfiguration.Height, Height);
            Width = konfiguration.Ink(konfiguration.Width, Width);

            tbaddr2line.Text = konfiguration.Addr2LineExe;
            TBStackdump.Text = konfiguration.Stackdump;
            TBElffile.Text = konfiguration.ElfFile;

            LBExceptionList.Items.Clear();
            LBExceptionList.ItemsSource = a2l.DataList;
        }

        //*****************************************************************************************
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            konfiguration.Top = Top;
            konfiguration.Left = Left;
            konfiguration.Height = Height;
            konfiguration.Width = Width;
            konfiguration.UIScale = uiScaler.ScaleX;

            konfiguration.Addr2LineExe = tbaddr2line.Text;
            konfiguration.Stackdump = TBStackdump.Text;
            konfiguration.ElfFile = TBElffile.Text;

            hlpr.SpeichereEinstellungen(konfiguration);
        }

        //*****************************************************************************************
        private void BTSelectExecutable(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Executable (*.exe)|*.exe|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
                tbaddr2line.Text = ofd.FileName;
        }

        //*****************************************************************************************
        private void BTFindExecutable(object sender, RoutedEventArgs e)
        {
            SetupHelper sh = new SetupHelper();
            tbaddr2line.ItemsSource = sh.findAddr2LineExe();
            tbaddr2line.SelectedIndex = 0;
        }

        //*****************************************************************************************
        private void BTSelectElffile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Elf-File (*.elf)|*.elf|All files (*.*)|*.*";
            if (ofd.ShowDialog() == true)
                TBElffile.Text = ofd.FileName;
        }

        //*****************************************************************************************
        private void BTFindElffile(object sender, RoutedEventArgs e)
        {
            ArduinoWorld aw = new ArduinoWorld();
            TBElffile.ItemsSource = aw.FindElfFile();
            TBElffile.SelectedIndex = 0;
        }

        //*****************************************************************************************
        private void BTAnalyze(object sender, RoutedEventArgs e)
        {
            a2l.Execute(tbaddr2line.Text, TBElffile.Text, TBStackdump.Text);

        }

        //*****************************************************************************************
        private void SenderDoppelClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TraceItem item = ((ListViewItem)sender).Content as TraceItem;

            string scf = item.SourcecodeFile;
            scf=scf.Replace("/","\\");

            if (File.Exists(scf))
            {
                TBSourceCodeFilecontent.Text = File.ReadAllText(scf);
                int linenr = int.Parse(item.SourcecodeLine);
                TBSourceCodeFilecontent.ScrollTo(linenr, 0);
            }
        }
    }
}
