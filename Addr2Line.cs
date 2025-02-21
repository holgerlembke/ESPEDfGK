using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Principal;

namespace ESPEDfGK
{
    // xtensa Instruction Set Architecture (ISA) Reference Manual.pdf
    // from 4.4.1.5 The Exception Cause Register (EXCCAUSE) under the Exception Option
    //*****************************************************************************************
    internal class ExceptionCausesEXCCAUSEitem
    {
        public int Start { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RequiredOption { get; set; }
        public int Ende { get; set; }
    }

    //*****************************************************************************************
    internal class ExceptionCausesEXCCAUSE
    {
        public ExceptionCausesEXCCAUSEitem[]? exceptions { get; set; }

        //*****************************************************************************************
        public ExceptionCausesEXCCAUSE()
        {
            string rn = @"ESPEDfGK.exceptioncodes.json";

            Assembly assembly = Assembly.GetExecutingAssembly();

            using (Stream? stream = assembly.GetManifestResourceStream(rn))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string s = reader.ReadToEnd();

                        exceptions = JsonConvert.DeserializeObject<ExceptionCausesEXCCAUSEitem[]>(s);
                    }
                }
            }
        }

        //*****************************************************************************************
        public ExceptionCausesEXCCAUSEitem? findExceptionCause(int id)
        {
            foreach (ExceptionCausesEXCCAUSEitem item in exceptions)
            {
                if ((item.Start == id) ||
                     ((item.Start >= id) && (item.Ende <= id)))
                {
                    return item;
                }
            }

            return null;
        }
        //*****************************************************************************************
        public ExceptionCausesEXCCAUSEitem? findExceptionCause(string id)
        {
            int hexcode = id.IndexOf("x");
            if (hexcode > -1)
            {
                id = id.Remove(0, hexcode + 1);
                if (int.TryParse(id, NumberStyles.HexNumber, null, out int idr))
                {
                    return findExceptionCause(idr);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if (int.TryParse(id, out int idr))
                {
                    return findExceptionCause(idr);
                }
                else
                {
                    return null;
                }
            }
        }
    }

    //*****************************************************************************************
    //*****************************************************************************************
    internal class TraceItem : INotifyPropertyChanged
    {
        public string Addr { get; set; }
        public string Name { get; set; }
        public string SourcecodeFile { get; set; }
        public string SourcecodeLine { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    //*****************************************************************************************
    //*****************************************************************************************
    internal abstract class Addr2LineBase
    {
        public ObservableCollection<TraceItem> DataList = new();
        protected Hashtable registers = new();
        public abstract void Execute(string addr2lineexe, string elffilename, string exceptiondump);
        public abstract string AnalyserType();

        public string? addr2lineoutputresult = null;
        public ExceptionCausesEXCCAUSEitem? exceptioncause = null;

        //*****************************************************************************************
        protected string subexec(string addr2lineexe, string elffilename, string addrlist)
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

            File.WriteAllText(@"j:\text.txt", output);

            addr2lineoutputresult = output;
            return output;
        }

        //*****************************************************************************************
        protected void prozessadd2lineoutput(string addr2lineexe, string elffilename, string BackTrace)
        {
            string output = subexec(addr2lineexe, elffilename, BackTrace);

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
                        lines[i] = lines[i].Remove(0, j + 2).Trim();
                    }

                    j = lines[i].IndexOf(" at ");
                    if (j >= 0)
                    {
                        ti.Name = lines[i].Substring(0, j).Trim();
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

    //*****************************************************************************************
    //*****************************************************************************************
    internal class Addr2LineEsp32 : Addr2LineBase
    {
        public override string AnalyserType()
        {
            return "ESP32";
        }

        //*****************************************************************************************
        private string findBacktrace(string exceptiondump)
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

        //*****************************************************************************************
        private void buildregistercollection(string exceptiondump)
        /*
esp32

PC      : 0x400f1441  PS      : 0x00060830  A0      : 0x800d1389  A1      : 0x3ffb2240  
A2      : 0x00000005  A3      : 0x00000003  A4      : 0x00000078  A5      : 0x00000003  
A6      : 0x00000001  A7      : 0x0000e100  A8      : 0x800d18c0  A9      : 0x3ffb2220  
A10     : 0x0000002a  A11     : 0x3f400120  A12     : 0x00000000  A13     : 0x00000000  
A14     : 0x3ffb7c80  A15     : 0x00000000  SAR     : 0x00000003  EXCCAUSE: 0x0000001c  
EXCVADDR: 0x0000002c  LBEG    : 0x400876b5  LEND    : 0x400876c5  LCOUNT  : 0xfffffffe  

Backtrace: 0x400f143e:0x3ffb2240 0x400d1386:0x3ffb2260 0x400d25d2:0x3ffb2290

Backtrace is PC:SP-pairs (program counter, stack pointer). sp is ignored by addr2line-tool.

https://docs.espressif.com/projects/esp-idf/en/latest/esp32/api-guides/fatal-errors.html


0x400f143e: exA() at D:\Arduino\sketches2.0\exceptiontestcode/exc1.ino:8
0x400d1386: exB() at D:\Arduino\sketches2.0\exceptiontestcode/exc1.ino:15
 (inlined by) exC() at D:\Arduino\sketches2.0\exceptiontestcode/exc1.ino:19
 (inlined by) setup() at D:\Arduino\sketches2.0\exceptiontestcode/exceptiontestcode.ino:9
0x400d25d2: loopTask(void *) at C:\Users\holger2\AppData\Local\Arduino15\packages\esp32\hardware\esp32\2.0.9\cores\esp32/main.cpp:42

         */
        {
            int i = exceptiondump.IndexOf("PC ");
            if (i > -1)
            {
                exceptiondump = exceptiondump.Remove(0, i);

                i = exceptiondump.IndexOf("LCOUNT");
                if (i > -1)
                {
                    exceptiondump = exceptiondump.Remove(i + 20);
                    exceptiondump = exceptiondump.Replace("\r\n", " ");
                    exceptiondump = exceptiondump.Replace(": ", " : ");

                    do
                    {
                        exceptiondump = exceptiondump.Replace("  ", " ");
                    } while (exceptiondump.Contains("  "));

                    exceptiondump = exceptiondump.Replace(" : ", ":");

                    string[] r = exceptiondump.Split(" ");

                    foreach (string s in r)
                    {
                        string[] l = s.Split(":");
                        registers.Add(l[0], l[1]);
                    }
                }
            }
        }

        //*****************************************************************************************
        public override void Execute(string addr2lineexe, string elffilename, string exceptiondump)
        {
            DataList.Clear();

            buildregistercollection(exceptiondump);

            if (registers.Count > 0)
            {
                // Wirklich?
                ExceptionCausesEXCCAUSE ec = new();
                string excause = (string)registers["EXCCAUSE"];
                if (excause != null)
                {
                    exceptioncause = ec.findExceptionCause(excause);
                }
            }
            // Backtrace trotzdem, z. b. bei 
            string bt = findBacktrace(exceptiondump);
            if (bt != "")
            {
                bt = registers["PC"] + ":" + registers["PC"] + " " + bt;
                prozessadd2lineoutput(addr2lineexe, elffilename, bt);
            }
        }
    }

    //*****************************************************************************************
    //*****************************************************************************************
    internal class Addr2LineEsp8266 : Addr2LineBase
    {
        private const string postfix = ":0xdead"; // not really needed, but...

        //*****************************************************************************************
        public override string AnalyserType()
        {
            return "ESP8266";
        }

        //*****************************************************************************************
        private string findBacktrace(string exceptiondump)
        {
            /*
            >>>stack>>>

            ctx: cont
            sp: 3ffffe20 end: 3fffffd0 offset: 0150
            3fffff70:  0000002a fffffffc 00000080 4020123c  
            3fffff80:  402011a4 3ffe87bc 3fffff90 4020103c <
            3fffff90:  3fffdad0 00000003 3fffffa0 40201058 <
            3fffffa0:  3fffdad0 00000000 3fffffb0 402010b0 <
            3fffffb0:  feefeffe feefeffe 3ffee578 40201a08  
            3fffffc0:  feefeffe feefeffe 3fffdab0 40100d19  
            <<<stack<<<
             */
            int i = exceptiondump.IndexOf(">>>stack>>>");
            if (i > -1)
            {
                exceptiondump = exceptiondump.Remove(0, i + 11);
                i = exceptiondump.IndexOf("<<<stack<<<");
                if (i > -1)
                {
                    exceptiondump = exceptiondump.Remove(i);

                    string[] lines = exceptiondump.Split("\r\n");

                    bool start = false;
                    string stack = "";
                    for (int j = 0; j < lines.Length; j++)
                    {
                        if (lines[j].StartsWith("sp:"))
                        {
                            start = true;
                        }
                        else // sicherstellen, dass es erst mit der nächsten Zeile weiter geht
                        {
                            if (start)
                            {
                                /* das ist flasch
                                string s = lines[j];
                                if (s.EndsWith(" <"))
                                {
                                    s = s.Remove(s.Length - 2);
                                    string[] v = s.Split(" ");

                                    stack = stack + " " + v[v.Length - 1] + postfix;
                                }
                                */
                                string s = lines[j];

                                if (s.IndexOf(":")>2)
                                {
                                    s = s.Remove(s.Length - 2);
                                    string[] v = s.Split(" ");

                                    if (v[v.Length - 1].StartsWith("4"))
                                    {
                                        stack = stack + " " + v[v.Length - 1] + postfix;
                                    }
                                }

                            }
                        }
                    }
                    return stack;
                }
            }

            return "";
        }

        //*****************************************************************************************
        private void buildregistercollection(string exceptiondump)
        {
            // epc1 = 0x4020413a epc2 = 0x00000000 epc3 = 0x00000000 excvaddr = 0x0000002c depc = 0x00000000

            int i = exceptiondump.IndexOf("epc1=");
            if (i > -1)
            {
                exceptiondump = exceptiondump.Remove(0, i);
                i = exceptiondump.IndexOf("\r\n");
                if (i > -1)
                {
                    exceptiondump = exceptiondump.Remove(i).Trim();
                    string[] r = exceptiondump.Split(" ");
                    foreach (string s in r)
                    {
                        string[] l = s.Split("=");
                        registers.Add(l[0], l[1]);
                    }
                }
            }
        }

        //*****************************************************************************************
        private void findexceptioncause(string exceptiondump)
        {
            /*
            Exception (9):
            epc1=0x402041b3 epc2=0x00000000 epc3=0x00000000 excvaddr=0x0000002a depc=0x00000000
             */
            int i = exceptiondump.IndexOf("Exception (");

            if (i > -1)
            {
                exceptiondump = exceptiondump.Remove(0, i + 11);
                i = exceptiondump.IndexOf(")");
                if (i > -1)
                {
                    exceptiondump = exceptiondump.Remove(i);

                    int exid;
                    if (int.TryParse(exceptiondump, out exid))
                    {
                        ExceptionCausesEXCCAUSE ec = new();
                        exceptioncause = ec.findExceptionCause(exid);
                    }
                }
            }
        }

        //*****************************************************************************************
        public override void Execute(string addr2lineexe, string elffilename, string exceptiondump)
        {
            DataList.Clear();

            buildregistercollection(exceptiondump);
            findexceptioncause(exceptiondump);

            string bt = findBacktrace(exceptiondump);

            bt = registers["epc1"] + postfix + " " + bt;
            if (bt != "")
            {
                prozessadd2lineoutput(addr2lineexe, elffilename, bt);
            }
        }
    }

    //*****************************************************************************************
    /// <summary>
    /// Detects whether a esp32 or esp8266 stack dump. Null for none. 
    /// </summary>
    internal class Addr2LineDecider
    {
        //*****************************************************************************************
        public Addr2LineBase? Decide(string exceptiondump)
        {
            if (exceptiondump.IndexOf(">stack>") > 0)
            {
                return new Addr2LineEsp8266();
            }
            else
            if (exceptiondump.IndexOf("register dump") > 0)
            {
                return new Addr2LineEsp32();
            }
            if (exceptiondump.IndexOf("CORRUPT HEAP: ") > 0)
            {
                return new Addr2LineEsp32();
            }
            if (exceptiondump.IndexOf("abort() was called") > 0)
            {
                return new Addr2LineEsp32();
            }
            // Fallback to last resort:
            if (exceptiondump.IndexOf("Backtrace: ") > 0)
            {
                return new Addr2LineEsp32();
            }

            // nothing found
            return null;
        }
    }
}
