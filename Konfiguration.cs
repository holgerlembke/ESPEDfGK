using Newtonsoft.Json;
using System;
using System.IO;

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
        public const string ArbeitsOrdner = "esp32exceptiondecoder";
        public const string KonfigFilename = "config.json";
        public const string LoggerDateiname = @"log %1.txt";
        public const string arduino15 = @"Arduino15\packages\";

        public const string arduinoclisettings = @".arduinoIDE\arduino-cli.yaml";
        public const string backtrace = @"Backtrace: ";

        public const string xtensaaddr2line = @"xtensa-*-addr2line.exe";
        public const string xtensaaddr2lineparam = @"-pfiaCr -e ""%1"" %2";
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
