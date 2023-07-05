using Microsoft.Win32;
using System.Configuration;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.IO;
using ICSharpCode.AvalonEdit;
using System.Linq.Expressions;
using System;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Threading;
using System.Reflection;

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

            Title = Title + " " + Assembly.GetExecutingAssembly().GetName().Version;

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

            LBStyleInfo.Content = null;
            LBExceptionInfo.Content = null;

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

            konfiguration.ElfFileSearchSpaceSketch = CBsearchpathSketch.IsChecked == true;
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
            tbaddr2line.IsDropDownOpen = true;
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
                CBsearchpathSketch.IsChecked == true, CBsearchpathTEMP.IsChecked == true);
            TBElffile.SelectedIndex = 0;
            TBElffile.IsDropDownOpen = true;
        }

        //*****************************************************************************************
        private void BTAnalyze(object sender, RoutedEventArgs e)
        {
            if (File.Exists(tbaddr2line.Text))
            {
                Addr2LineDecider decider = new();
                Addr2LineBase? analyzer = decider.Decide(TBStackdump.Text);

                if (analyzer == null)
                {
                    return;
                }

                LBStyleInfo.Content = "CPU is: " + analyzer.AnalyserType();

                analyzer.Execute(tbaddr2line.Text, TBElffile.Text, TBStackdump.Text);

                BTnCopyToClipboard.IsEnabled = true;

                LBExceptionList.ItemsSource = null;
                LBExceptionList.Items.Clear(); // der xaml-code könnnte daten liefern....
                LBExceptionList.ItemsSource = analyzer.DataList;

                // Exception Ursache
                if (analyzer.exceptioncause != null)
                {
                    LBExceptionInfo.Content = "EC: " + analyzer.exceptioncause.Description;
                }
                else
                {
                    LBExceptionInfo.Content = "";
                }

                // Wenn nix rauskommt, dann das Ausführungsergebnis anzeigen
                if (analyzer.DataList.Count == 0)
                {
                    TBSourceCodeFilecontent.Text = analyzer.addr2lineoutputresult;
                }
                else
                {
                    TBSourceCodeFilecontent.Text = "";
                }
            }
            else
            {
                LBStyleInfo.Content = "Settings: addr2line is invalid.";
                BTnCopyToClipboard.IsEnabled = true;
            }
        }

        //*****************************************************************************************
        private void SenderDoppelClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TraceItem? item = ((ListViewItem)sender).Content as TraceItem;

            if (item != null)
            {
                string scf = item.SourcecodeFile;
                scf = scf.Replace("/", "\\");

                if (File.Exists(scf))
                {
                    int linenr = int.Parse(item.SourcecodeLine);

                    HCLBR.LineNumber = linenr;

                    try
                    {
                        TBSourceCodeFilecontent.Text = File.ReadAllText(scf);
                        TBSourceCodeFilecontent.ScrollTo(linenr, 0);
                    }
                    catch
                    {
                        TBSourceCodeFilecontent.Text = "Can not access file.";
                    }
                }
            }
        }

        //*****************************************************************************************
        private void BTCopyToClipboard(object sender, RoutedEventArgs e)
        {
            string clpboardtext = (string)LBStyleInfo.Content +
                                   Environment.NewLine +
                                   LBExceptionInfo.Content +
                                   Environment.NewLine + Environment.NewLine;

            foreach (TraceItem ti in LBExceptionList.ItemsSource)
            {
                clpboardtext += ti.Addr+" "+
                                ti.Name + " "+
                                ti.SourcecodeFile + " "+
                                ti.SourcecodeLine + " "+
                                Environment.NewLine;
            }
            Clipboard.SetText(clpboardtext);

            // Visuelle Rückkopplung der Aktion
            BTnCopyToClipboard.Visibility= Visibility.Collapsed;
            BTnCopyToClipboardDone.Visibility= Visibility.Visible;
            DispatcherTimer timer = new();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += (s, a) =>
            {
                BTnCopyToClipboard.Visibility = Visibility.Visible;
                BTnCopyToClipboardDone.Visibility = Visibility.Collapsed;
                timer.Stop();
            };
            timer.Start();
        }
    }
}
