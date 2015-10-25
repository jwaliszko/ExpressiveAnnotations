using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public static class ServerStarter
    {
        private static readonly object _locker = new object();
        private static Process _iisProcess;

        public static void Start(int port)
        {
            if (_iisProcess != null)
            {
                Wait();
                return;
            }

            lock (_locker)
            {
                if (_iisProcess != null)
                {
                    Wait();
                    return;
                }

                var applicationPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\ExpressiveAnnotations.MvcWebSample"));
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                _iisProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = programFiles + @"\IIS Express\iisexpress.exe",
                        Arguments = string.Format("/path:{0} /port:{1}", applicationPath, port)
                    }
                };
                _iisProcess.Start();
                AppDomain.CurrentDomain.DomainUnload += Shutdown;
                Wait();
            }            
        }

        private static void Shutdown(object sender, EventArgs e)
        {
            Halt();
        }

        private static void Wait()
        {
            while (Process.GetProcesses().All(p => p.ProcessName != "iisexpress"))
            {
                Thread.Sleep(100);
            }
        }

        public static void Halt()
        {
            if (_iisProcess == null) 
                return;

            AppDomain.CurrentDomain.DomainUnload -= Shutdown;
            _iisProcess.Kill();
            _iisProcess.Dispose(); // msdn: The Dispose method calls Close.
            _iisProcess = null;
        }
    }
}
