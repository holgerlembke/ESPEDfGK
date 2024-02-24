using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.IO;
using System.Linq;

namespace ESPEDfGK
{
    //*****************************************************************************************
    internal class FileListGetter
    {
        //*****************************************************************************************
        public string[] filelist(string path, string pattern)
        {
            try
            {
                return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
            }
            catch
            {
                return new string[0];
            }
        }

        //*****************************************************************************************
        public string[] filelistNewAtTop(string[] fl)
        {
            if (fl.Length > 1) // eine einfache suche, erstes element austauschen
            {
                int i = 0;
                DateTime md = File.GetLastWriteTime(fl[0]);

                for (int j = 1; j < fl.Length; j++)
                {
                    DateTime nmd = File.GetLastWriteTime(fl[j]);
                    if (nmd > md)
                    {
                        i = j;
                        md = nmd;
                    }
                }

                if (i > 0)
                {
                    string s = fl[0];
                    fl[0] = fl[i];
                    fl[i] = s;
                }
            }

            return fl;
        }
    }

    //*****************************************************************************************
    internal class SetupHelper
    {
        //*****************************************************************************************
        public string[] findAddr2LineExe()
        {
            string pArduino = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                Path.DirectorySeparatorChar + StringContent.arduino15;
            
            string pPlatformIO = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + 
                Path.DirectorySeparatorChar + StringContent.platformio;

            FileListGetter flg = new();

            string[] res = flg.filelist(pArduino, StringContent.xtensaaddr2line);

            string[] resPIO = flg.filelist(pPlatformIO, StringContent.xtensaaddr2line);

            res = res.Union(resPIO).ToArray();  

            return res;
        }
    }
}
