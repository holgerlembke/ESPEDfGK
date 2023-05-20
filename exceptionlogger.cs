using System;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace ESPEDfGK
{
    // // https://asiablog.acumatica.com/2015/09/log-specific-exceptions-with-first.html
    internal class ExceptionLogger
    {
        private string filename;
        private const uint HRFileLocked = 0x80070020;
        private const uint HRPortionOfFileLocked = 0x80070021;

        //******************************************************************************************************************
        public ExceptionLogger(string filename)
        {
            this.filename = filename;
            AppDomain.CurrentDomain.FirstChanceException += OnCurrentDomainOnFirstChanceException;
        }

        [ThreadStatic]
        private bool IsRecursive;

        //******************************************************************************************************************
        private void OnCurrentDomainOnFirstChanceException(object o, FirstChanceExceptionEventArgs args)
        {
            if (IsRecursive)
            {
                return;
            }

            if (args.Exception is IOException)
            {
                return;
            }

            try
            {
                IsRecursive = true;
                LogException(args.Exception);
            }
            catch
            {
                // prevent stack overflow
            }
            finally
            {
                IsRecursive = false;
            }
        }

        //******************************************************************************************************************
        private void LogException(Exception exception)
        {
            StackTrace trace = new StackTrace(2);

            Stream stream;
            while (!TryOpen(filename, out stream))
            {
                Thread.Sleep(100); // wait for file to unlock
            }
            using (stream)
            {
                //Write your log here
                StreamWriter sw = new StreamWriter(stream);
                sw.Write("\n======== ");
                sw.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff "));
                sw.Write(" ========");
                sw.WriteLine();

                sw.WriteLine("Message: " + exception.Message);
                sw.WriteLine();
                sw.WriteLine(trace.ToString());
                sw.WriteLine();

                sw.Close();
            }
        }

        //******************************************************************************************************************
        private bool FileIsLocked(IOException ioException)
        {
            var errorCode = (uint)Marshal.GetHRForException(ioException);
            return errorCode == HRFileLocked || errorCode == HRPortionOfFileLocked;
        }

        //******************************************************************************************************************
        private bool TryOpen(string path, out Stream stream)
        {
            try
            {
                stream = File.Open(path, FileMode.Append);
                return true;
            }
            catch (IOException e)
            {
                if (!FileIsLocked(e))
                    throw;

                stream = null;
                return false;
            }
        }
    }
}