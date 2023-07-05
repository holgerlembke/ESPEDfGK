using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ESPEDfGK
{
    //*****************************************************************************************
    public static class ExitErrorCode
    {
        private const int Error = 100;
        public const int NoCFG = Error + 0;
        public const int CFGLoadFail = Error + 1;
    }

    //*****************************************************************************************
    public static class StringContent
    {
        public const string ArbeitsOrdner = "EspExceptionDecoder";
        public const string KonfigFilename = "config.json";
        public const string LoggerDateiname = @"log %1.txt";

        // Orte, wo die xtensaaddr2line sein könnten
        public const string arduino15 = @"Arduino15\packages\";
        public const string platformio = @".platformio\packages\";

        public const string startmutex = "Global\\FB0B2193-43C8-4DC2-8CE3-51E9C4E97C24";

        public const string arduinoclisettings = @".arduinoIDE\arduino-cli.yaml";
        public const string backtrace = @"Backtrace: ";

        public const string xtensaaddr2line = @"xtensa-*-addr2line.exe";
        public const string xtensaaddr2lineparam = @"-pfiaCr -e ""%1"" %2";
        public const string elffilepattern = @"*.elf";
        /*
         Convert addresses into line number/file name pairs.
         If no addresses are specified on the command line, they will be read from stdin
         The options are:
          @<file>                Read options from<file>
          -a --addresses Show addresses
          -b --target=< bfdname > Set the binary file format
          -e --exe=< executable > Set the input file name (default is a.out)
          -i --inlines Unwind inlined functions
          -j --section=<name>    Read section-relative offsets instead of addresses
          -p --pretty-print Make the output easier to read for humans
          -s --basenames Strip directory names
          -f --functions Show function names
          -C --demangle[= style] Demangle function names
          -R --recurse-limit Enable a limit on recursion whilst demangling.  [Default]
          -r --no-recurse-limit Disable a limit on recursion whilst demangling
          -h --help Display this information
          -v --version Display the program's version
        */
    }

    //*****************************************************************************************
    class Konfiguration
    {
        public double UIScale { get; set; }

        public double Top { get; set; }
        public double Left { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public string Addr2LineExe { get; set; }
        public string Stackdump { get; set; }
        public string ElfFile { get; set; }

        public double SplitHeight { get; set; }

        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ElfFileSearchSpaceSketch { get; set; }
        [DefaultValue(true)]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public bool ElfFileSearchSpaceTEMP { get; set; }

        //*****************************************************************************************
        //*****************************************************************************************
        //*****************************************************************************************
        public double Ink(double v, double vorgabe) // IstNichtKlein
        {
            return (Math.Abs(v) < 0.001) ? vorgabe : v;
        }

        //*****************************************************************************************
        private string ArbeitsOrdnerName()
        {
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData,
                                                       Environment.SpecialFolderOption.Create);

            roaming += Path.DirectorySeparatorChar + StringContent.ArbeitsOrdner;
            Directory.CreateDirectory(roaming);

            return roaming;
        }

        //*****************************************************************************************
        public string LoggerDateiname()
        {
            string f = ArbeitsOrdnerName() + Path.DirectorySeparatorChar + StringContent.LoggerDateiname;

            f = f.Replace("%1", DateTime.Now.ToString("yyyy-MM-dd"));

            return f;
        }

        //*****************************************************************************************
        public string KonfigFileName()
        {
            return ArbeitsOrdnerName() + Path.DirectorySeparatorChar + StringContent.KonfigFilename;
        }
    }


    //*****************************************************************************************
    class KonfigurationsHlpr
    {
        //*****************************************************************************************
        public Konfiguration LadeEinstellungen()
        {
            Konfiguration r = new Konfiguration();

            if (File.Exists(r.KonfigFileName()))
            {
                try
                {
                    using (StreamReader file = File.OpenText(r.KonfigFileName()))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        r = (Konfiguration)serializer.Deserialize(file, typeof(Konfiguration));

                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine("Panic: Loading CFG failed with {0}", e.Message);
                    Environment.Exit(ExitErrorCode.CFGLoadFail);
                }
            }

            if (r == null)
            {
                r = new Konfiguration();
            }

            return r;
        }

        //*****************************************************************************************
        public void SpeichereEinstellungen(Konfiguration k)
        {
            string s = JsonConvert.SerializeObject(k, Formatting.Indented);
            File.WriteAllText(k.KonfigFileName(), s);
        }

    }

}
