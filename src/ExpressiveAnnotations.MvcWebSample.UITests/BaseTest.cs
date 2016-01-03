using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using OpenQA.Selenium.Remote;
using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public abstract class BaseTest : IDisposable, IClassFixture<DriverFixture>, IAssemblyFixture<ServerFixture>
    {
        private readonly RemoteWebDriver _driver;

        protected BaseTest(DriverFixture classContext, ServerFixture assemblyContext) // called before every test method
        {
            _driver = classContext.Driver;
            Home = new HomePage(classContext.Driver);
            Home.Load($"http://localhost:{assemblyContext.Port}/");
        }

        public HomePage Home { get; private set; }

        protected void Watch(Action action, [CallerMemberName] string name = null) // no context available in xUnit to recognize failing tests -
        {                                                                          // - wrapper function used instead (Higher order programming)
            try
            {
                action();
            }
            catch
            {
                TakeScreenshot(name);
                throw;
            }
        }

        protected void TakeScreenshot(string name)
        {
            var directory = Directory.CreateDirectory("Screenshots");
            _driver.GetScreenshot().SaveAsFile($"{directory.Name}/{name}.png", ImageFormat.Png);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Home != null)
                {
                    Home.Clean();
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
