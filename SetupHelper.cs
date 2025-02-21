using System;
using System.Collections.ObjectModel;
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
    internal class Addr2LineInfo
    {
        public string filepath { get; set; }
        public string fileinfo { get; set; }
    }

    //*****************************************************************************************
    internal class Addr2LineList : ObservableCollection<Addr2LineInfo> { }

    //*****************************************************************************************
    internal class SetupHelper
    {
        //*****************************************************************************************
        public Addr2LineList findAddr2LineExe()
        {
            string pArduino = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                                Path.DirectorySeparatorChar + StringContent.arduino15;

            string pPlatformIO = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                Path.DirectorySeparatorChar + StringContent.platformio;

            FileListGetter flg = new();

            string[] res = flg.filelist(pArduino, StringContent.xtensaaddr2line);

            string[] resPIO = flg.filelist(pPlatformIO, StringContent.xtensaaddr2line);

            res = res.Union(resPIO).ToArray();

            // Build list
            Addr2LineList reslist = new();
            foreach (string f in res)
            {
                Addr2LineInfo info = new();
                info.filepath = f;
                info.fileinfo = File.GetCreationTime(f).ToString() + " " +
                               (new System.IO.FileInfo(f).Length).ToString()+" B";

                reslist.Add(info);
            }

            return reslist;
        }
    }
}
