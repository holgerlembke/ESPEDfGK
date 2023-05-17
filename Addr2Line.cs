using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using System.Windows.Documents;

namespace ESPEDfGK
{
    class TraceItem : INotifyPropertyChanged
    {
        public string Addr { get; set; }
        public string Name { get; set; }
        public string SourcecodeFile { get; set; }
        public string SourcecodeLine { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    class Addr2Line
    {
        public ObservableCollection<TraceItem> DataList = new();

        //*****************************************************************************************
        string subexec(string addr2lineexe, string elffilename, string addrlist)
        {
            // Start the child process.
            Process p = new Process();
            // Redirect the output stream of the child process.
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = addr2lineexe;

            string args = StringContent.xtensaaddr2lineparam;
            args = args.Replace("%1", elffilename);
            args = args.Replace("%2", addrlist);

            p.StartInfo.Arguments = args;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            return output;
        }

        //*****************************************************************************************
        string findBacktrace(string exceptiondump)
        {
            int i = exceptiondump.IndexOf(StringContent.backtrace);

            if (i > 0)
            {
                exceptiondump = exceptiondump.Remove(0, i + StringContent.backtrace.Length);

                i = exceptiondump.IndexOf("\r");
                if (i > 0)
                {
                    exceptiondump = exceptiondump.Substring(0, i);
                }
                return exceptiondump;
            }
            return "";
        }

        // Backtrace: 0x400f143e:0x3ffb2240 0x400d1386:0x3ffb2260 0x400d25d2:0x3ffb2290


        //*****************************************************************************************
        public void Execute(string addr2lineexe, string elffilename, string exceptiondump)
        {
            DataList.Clear();

            string bt = findBacktrace(exceptiondump);
            if (bt != "")
            {
                string output = subexec(addr2lineexe, elffilename, bt);

                string[] lines = output.Split("\r\n");

                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i] = lines[i].Trim();

                    if (lines[i] != "")
                    {
                        TraceItem ti = new();
                        int j = lines[i].IndexOf(": ");
                        if (j == -1)
                        {
                            // (inlined by)
                            j = lines[i].IndexOf("(inlined by)");
                            if (j > -1)
                            {
                                lines[i] = lines[i].Remove(j, 12).Trim();
                                ti.Addr = "inlined by ";
                            }
                        }
                        else
                        {
                            ti.Addr = lines[i].Substring(0, j);
                            lines[i] = lines[i].Remove(0, j + 2);
                        }

                        j = lines[i].IndexOf(" at ");
                        ti.Name = lines[i].Substring(0, j);
                        lines[i] = lines[i].Remove(0, j + 3);

                        j = lines[i].Length - 1;
                        while ((j > 0) && (lines[i][j] != ':'))
                        {
                            j--;
                        }
                        ti.SourcecodeFile = lines[i].Substring(0, j).Trim();
                        ti.SourcecodeLine = lines[i].Substring(j + 1, lines[i].Length - j - 1).Trim();

                        DataList.Add(ti);
                    }
                }
            }
        }
    }
}

/*
0x400f143e: exA() at D:\Arduino\sketches2.0\exceptiontestcode/exc1.ino:8
0x400d1386: exB() at D:\Arduino\sketches2.0\exceptiontestcode/exc1.ino:15
 (inlined by) exC() at D:\Arduino\sketches2.0\exceptiontestcode/exc1.ino:19
 (inlined by) setup() at D:\Arduino\sketches2.0\exceptiontestcode/exceptiontestcode.ino:9
0x400d25d2: loopTask(void *) at C:\Users\holger2\AppData\Local\Arduino15\packages\esp32\hardware\esp32\2.0.9\cores\esp32/main.cpp:42
*/


