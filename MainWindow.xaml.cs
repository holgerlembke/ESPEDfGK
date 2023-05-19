using Microsoft.Win32;
using System.Configuration;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using ICSharpCode.AvalonEdit;

namespace ESPEDfGK
{
    //*****************************************************************************************
    public partial class MainWindow : Window
    {
        Konfiguration konfiguration = null;
        KonfigurationsHlpr hlpr = new();
        HighlightCurrentLineBackgroundRenderer HCLBR;

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

            // Zeile visualisieren
            HCLBR = new HighlightCurrentLineBackgroundRenderer(TBSourceCodeFilecontent);
            TBSourceCodeFilecontent.TextArea.TextView.BackgroundRenderers.Add(HCLBR);

            CBsearchpathSketch.IsChecked = konfiguration.ElfFileSearchSpaceSketch == true;
            CBsearchpathTEMP.IsChecked = konfiguration.ElfFileSearchSpaceTEMP == true;

            tbaddr2line.Text = konfiguration.Addr2LineExe;
            TBStackdump.Text = konfiguration.Stackdump;
            TBElffile.Text = konfiguration.ElfFile;
            TBStackdump.Height = konfiguration.Ink(konfiguration.SplitHeight, TBStackdump.Height);
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
            konfiguration.SplitHeight = TBStackdump.Height;

            konfiguration.ElfFileSearchSpaceSketch = CBsearchpathSketch.IsChecked==true;
            konfiguration.ElfFileSearchSpaceTEMP = CBsearchpathTEMP.IsChecked == true;

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
            ElfListGetter getter = new();
            TBElffile.DataContext = getter.GetFileList(
                CBsearchpathSketch.IsChecked == true,CBsearchpathTEMP.IsChecked == true);
            TBElffile.SelectedIndex = 0;
            TBElffile.IsDropDownOpen = true;
        }

        //*****************************************************************************************
        private void BTAnalyze(object sender, RoutedEventArgs e)
        {
            Addr2LineDecider decider = new();
            Addr2LineBase analyzer = decider.Decide(TBStackdump.Text);

            LBStyleInfo.Content = "Dump intepreted as: " + analyzer.AnalyserType();

            analyzer.Execute(tbaddr2line.Text, TBElffile.Text, TBStackdump.Text);

            LBExceptionList.ItemsSource = null;
            LBExceptionList.Items.Clear(); // der xaml-code könnnte daten liefern....
            LBExceptionList.ItemsSource = analyzer.DataList;
        }

        //*****************************************************************************************
        private void SenderDoppelClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TraceItem item = ((ListViewItem)sender).Content as TraceItem;

            string scf = item.SourcecodeFile;
            scf = scf.Replace("/", "\\");

            if (File.Exists(scf))
            {
                int linenr = int.Parse(item.SourcecodeLine);

                HCLBR.LineNumber = linenr;
                TBSourceCodeFilecontent.Text = File.ReadAllText(scf);
                TBSourceCodeFilecontent.ScrollTo(linenr, 0);
            }
        }
    }
}
