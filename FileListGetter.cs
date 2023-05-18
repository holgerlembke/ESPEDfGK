using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPEDfGK
{
    //*****************************************************************************************
    public class FileListGetter
    {
        //*****************************************************************************************
        public string[] filelist(string path, string pattern, bool sortnewatzero)
        {
            string[] allFiles = Directory.GetFiles(path, pattern, SearchOption.AllDirectories);

            if ((allFiles.Length>1) && (sortnewatzero)) // eine einfache suche, erstes element austauschen
            {
                int i = 0;
                DateTime md = File.GetLastWriteTime(allFiles[0]);

                for (int j=1; j<allFiles.Length; j++)
                {
                    DateTime nmd = File.GetLastWriteTime(allFiles[j]);
                    if (nmd>md)
                    {
                        i = j;
                        md = nmd;
                    }
                }

                if (i>0)
                {
                    string s = allFiles[0];
                    allFiles[0]= allFiles[i];
                    allFiles[i] = s;
                }
            }

            return allFiles;
        }
    }
}
