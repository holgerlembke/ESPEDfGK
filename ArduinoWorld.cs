using System;
using System.IO;
using System.Windows.Media.TextFormatting;

namespace ESPEDfGK
{
    class ArduinoWorld
    {

        //*****************************************************************************************
        string SketchFolder()
        {
            string p = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
                + Path.DirectorySeparatorChar + StringContent.arduinoclisettings;

            string[] content = File.ReadAllLines(p);
            /*

downloads: c:\Users\holger2\AppData\Local\Arduino15\staging
  user: d:\Arduino\sketches2.0             
             

             */
            for (int i=0; i<content.Length; i++)
            {
                int j = content[i].IndexOf("user:");
                if (j>0)
                {
                    string s = content[i].Substring(j + 5);
                    return s.Trim();
                }
            }

            return ""; // nix gefunden
        }

        //*****************************************************************************************
        public string[] FindElfFile()
        {
            string p = SketchFolder();
            string[] allFiles = Directory.GetFiles(p, "*.elf", SearchOption.AllDirectories);
            return allFiles;
        }

    }



}
