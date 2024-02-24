using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace ESPEDfGK
{

    //*****************************************************************************************
    // next step: c# wpf add tooltip for combobox item
    internal class FileListItem : INotifyPropertyChanged
    {
        public string FileName { get; set; }
        public DateTime LastChange { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;

        public override string ToString()
        {
            return FileName;
        }
    }

    //*****************************************************************************************
    internal class ElfListGetter
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
            for (int i = 0; i < content.Length; i++)
            {
                int j = content[i].IndexOf("user:");
                if (j > 0)
                {
                    string s = content[i].Substring(j + 5);
                    return s.Trim();
                }
            }

            return ""; // nix gefunden
        }

        //*****************************************************************************************
        string TempFolder()
        {
            return Path.GetTempPath();
        }

        //*****************************************************************************************
        public ObservableCollection<FileListItem> GetFileList(bool usesketchfolder, bool usetempfolder)
        {
            ObservableCollection<FileListItem> res = new();

            List<string> pl = new();

            if (usesketchfolder)
            {
                pl.Add(SketchFolder());
            }
            if (usetempfolder)
            {
                pl.Add(TempFolder());
            }
            if (pl.Count==0) // stupid configuration, no location selected... ?
            {
                pl.Add(SketchFolder());
                pl.Add(TempFolder());
            }

            foreach (string p in pl)
            {
                try
                {
                    string[] files = Directory.GetFiles(p, StringContent.elffilepattern, SearchOption.AllDirectories);

                    foreach (string f in files)
                    {
                        FileListItem item = new();
                        item.FileName = f;
                        item.LastChange = File.GetLastWriteTime(f);
                        res.Add(item);
                    }
                }
                catch
                {
                    //;
                }
            }

            if (res.Count > 1)
            {
                int i = 0;
                DateTime md = res[i].LastChange;

                for (int j = 1; j < res.Count; j++)
                {
                    if (res[j].LastChange > md)
                    {
                        md = res[j].LastChange;
                        i = j;
                    }
                }
                res.Insert(0, res[i]);
                res.RemoveAt(i+1);
            }

            return res;
        }
    }
}
