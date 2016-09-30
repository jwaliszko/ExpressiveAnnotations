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

            _iisProcess = new Process
            {
                StartInfo =
                {
                    FileName = @"C:\Program Files\IIS Express\iisexpress.exe",
                    Arguments = $@"/path:""{applicationPath}"" /port:{Port}"
                }
            };
            _iisProcess.Start();

            const string screenshots = "Screenshots";
            if (Directory.Exists(screenshots))
                Directory.Delete(screenshots, true); // remove existing screenshots of previously failing tests, if any
        }

        public int Port { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_iisProcess != null)
                {
                    try
                    {
                        _iisProcess.Kill();
                    }
                    catch
                    {
                        // ignored
                    }
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
