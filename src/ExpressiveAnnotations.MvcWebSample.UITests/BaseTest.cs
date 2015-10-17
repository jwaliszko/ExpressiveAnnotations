using System;
using System.Diagnostics;
using System.IO;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public abstract class BaseTest : IDisposable
    {
        private static readonly object _locker = new object();
        private static Process _iisProcess;

        protected BaseTest() // called before every test method
        {
            StartIIS();
            Home = new HomePage();
            Home.NavigateToSelf();
        }

        public HomePage Home { get; private set; }

        private void StartIIS()
        {
            if (_iisProcess != null) return;
            lock (_locker)
            {
                if (_iisProcess != null) return;

                var applicationPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\ExpressiveAnnotations.MvcWebSample"));
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                _iisProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = programFiles + @"\IIS Express\iisexpress.exe",
                        Arguments = string.Format("/path:{0} /port:{1}", applicationPath, 51622)
                    }
                };
                _iisProcess.Start();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Home != null)
                {
                    Home.Dispose();
                    Home = null;
                }
            }
        }

        public void Dispose() // called after every test method
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
