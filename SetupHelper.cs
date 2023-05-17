using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESPEDfGK
{
    class SetupHelper
    {
        public string[] findAddr2LineExe()
        {
            string p = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
                + Path.DirectorySeparatorChar+
                    StringContent.arduino15;

            string[] allFiles = Directory.GetFiles(p, StringContent.xtensaaddr2line, 
                                                      SearchOption.AllDirectories);

            return allFiles;
        }
    }
}
