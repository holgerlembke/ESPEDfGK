using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ESPEDfGK
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static System.Threading.Mutex mutex = null;

        //*****************************************************************************************
        protected override void OnStartup(StartupEventArgs e)
        {
            mutex = new System.Threading.Mutex(true, StringContent.startmutex, out bool createdNew);
            if (!createdNew)
            {
                Current.Shutdown();
            }
            else
            {
                Exit += CloseMutexHandler;
            }
            base.OnStartup(e);
        }
        //*****************************************************************************************
        protected virtual void CloseMutexHandler(object sender, EventArgs e)
        {
            mutex?.Close();
        }

    }
}
