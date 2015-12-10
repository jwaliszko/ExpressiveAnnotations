using System;
using System.Diagnostics;
using System.IO;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class ServerFixture : IDisposable
    {
        private Process _iisProcess;

        public ServerFixture() // called before all tests
        {
            Port = 51622;
            var applicationPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\ExpressiveAnnotations.MvcWebSample"));
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            _iisProcess = new Process
            {
                StartInfo =
                {
                    FileName = programFiles + @"\IIS Express\iisexpress.exe",
                    Arguments = $"/path:{applicationPath} /port:{Port}"
                }
            };
            _iisProcess.Start();
        }

        public int Port { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_iisProcess != null)
                {
                    _iisProcess.Kill();
                    _iisProcess.Dispose(); // MSDN: the Dispose() method calls Close()
                    _iisProcess = null;
                }
            }
        }

        public void Dispose() // called after all tests
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
