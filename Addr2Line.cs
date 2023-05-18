using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Automation;
using System.Windows.Documents;

namespace ESPEDfGK
{
    public class ExceptionnStringList
    {
        // https://links2004.github.io/Arduino/dc/deb/md_esp8266_doc_exception_causes.html
        public readonly string[] list =
        {
            @"Illegal instruction",
            @"SYSCALL instruction",
            @"InstructionFetchError: Processor internal physical address or data error during instruction fetch",
            @"LoadStoreError: Processor internal physical address or data error during load or store",
            @"Level1Interrupt: Level-1 interrupt as indicated by set level-1 bits in the INTERRUPT register",
            @"Alloca: MOVSP instruction, if caller's registers are not in the register file",
            @"IntegerDivideByZero: QUOS, QUOU, REMS, or REMU divisor operand is zero",
            @"reserved",
            @"Privileged: Attempt to execute a privileged operation when CRING ? 0",
            @"LoadStoreAlignmentCause: Load or store to an unaligned address",
            @"reserved",
            @"reserved",
            @"InstrPIFDataError: PIF data error during instruction fetch",
            @"LoadStorePIFDataError: Synchronous PIF data error during LoadStore access",
            @"InstrPIFAddrError: PIF address error during instruction fetch",
            @"LoadStorePIFAddrError: Synchronous PIF address error during LoadStore access",
            @"InstTLBMiss: Error during Instruction TLB refill",
            @"InstTLBMultiHit: Multiple instruction TLB entries matched",
            @"InstFetchPrivilege: An instruction fetch referenced a virtual address at a ring level less than CRING",
            @"reserved",
            @"InstFetchProhibited: An instruction fetch referenced a page mapped with an attribute that does not permit instruction fetch",
            @"reserved",
            @"reserved",
            @"reserved",
            @"LoadStoreTLBMiss: Error during TLB refill for a load or store",
            @"LoadStoreTLBMultiHit: Multiple TLB entries matched for a load or store",
            @"LoadStorePrivilege: A load or store referenced a virtual address at a ring level less than CRING",
            @"reserved",
            @"LoadProhibited: A load referenced a page mapped with an attribute that does not permit loads",
            @"StoreProhibited: A store referenced a page mapped with an attribute that does not permit stores"
        };
    }

    //*****************************************************************************************
    //*****************************************************************************************
    class TraceItem : INotifyPropertyChanged
    {
        public string Addr { get; set; }
        public string Name { get; set; }
        public string SourcecodeFile { get; set; }
        public string SourcecodeLine { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    //*****************************************************************************************
    //*****************************************************************************************
    abstract class Addr2LineBase
    {
        public ObservableCollection<TraceItem> DataList = new();
        protected Hashtable registers = new();
        public abstract void Execute(string addr2lineexe, string elffilename, string exceptiondump);
        public abstract string AnalyserType();

        public string addr2lineoutputresult;

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
    class Addr2LineEsp32 : Addr2LineBase
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
    class Addr2LineEsp8266 : Addr2LineBase
    {
        private const string postfix = ":0xdead";

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
                                string s = lines[j];
                                if (s.EndsWith(" <"))
                                {
                                    s = s.Remove(s.Length - 2);
                                    string[] v = s.Split(" ");

                                    stack = stack + " " + v[v.Length - 1] + postfix;
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
        public override void Execute(string addr2lineexe, string elffilename, string exceptiondump)
        {
            DataList.Clear();

            buildregistercollection(exceptiondump);

            string bt = findBacktrace(exceptiondump);

            bt = registers["epc1"] + postfix + " " + bt;
            if (bt != "")
            {
                // bt = registers["PC"] + ":" + registers["PC"] + " " + bt;
                prozessadd2lineoutput(addr2lineexe, elffilename, bt);
            }
        }
    }

    //*****************************************************************************************
    // esp32 or esp8288 stack dumpp?
    class Addr2LineDecider
    {
        public Addr2LineBase Decide(string exceptiondump)
        {
            if (exceptiondump.IndexOf(">stack>") > 0)
            {
                return new Addr2LineEsp8266();
            }
            else
            {
                return new Addr2LineEsp32();
            }
            return null; // nah. :-)
        }
    }
}
