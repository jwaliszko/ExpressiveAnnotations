using System;
using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public abstract class BaseTest : IDisposable, IClassFixture<DriverFixture>
    {
        protected BaseTest(DriverFixture context) // called before every test method
        {
            const int port = 51622;
            ServerStarter.Start(port);
            Home = new HomePage(context.Driver);
            Home.Load(string.Format("http://localhost:{0}/", port));
        }

        public HomePage Home { get; private set; }

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
