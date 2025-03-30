using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace ESPEDfGK
{
    class ExceptionFileMonitor
    {
        //*****************************************************************************************
        public ExceptionFileMonitor(string FileMonitored)
        {
            this.fileMonitored = FileMonitored;
        }

        string fileMonitored;
        public string FileMonitored { get { return fileMonitored; } }


        private bool die = false;
        public bool Die { set { die = value; } }

        //*****************************************************************************************
        public event EventHandler ExceptionFileFound = null;

        //*****************************************************************************************
        void job()
        {
            FileInfo fi = null;
            do
            {
                FileInfo nfi = new FileInfo(FileMonitored);

                if ((nfi.Exists) && (fi == null))
                {
                    OnExceptionFileFound(EventArgs.Empty);
                }
                else
                // wir haben schon mal so eine Datei gesehen...
                if (fi != null)
                {
                    if ((nfi.CreationTime != fi.CreationTime) ||
                        (nfi.LastWriteTime != fi.LastWriteTime)
                       )
                    {
                        OnExceptionFileFound(EventArgs.Empty);
                    }
                }

                fi = nfi;
                Thread.Sleep(100);
            } while (!die);
        }

        //*****************************************************************************************
        protected virtual void OnExceptionFileFound(EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                () => ExceptionFileFound?.Invoke(this, e)
            );
        }

        //*****************************************************************************************
        public void start()
        {
            Thread thread = new(job);
            thread.Start();
        }
    }
}

