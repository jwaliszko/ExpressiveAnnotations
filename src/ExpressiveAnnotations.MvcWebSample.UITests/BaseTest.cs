using System;
using Xunit;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public abstract class BaseTest : IDisposable, IClassFixture<DriverFixture>, IAssemblyFixture<ServerFixture>
    {
        protected BaseTest(DriverFixture classContext, ServerFixture assemblyContext) // called before every test method
        {
            Home = new HomePage(classContext.Driver);
            Home.Load(string.Format("http://localhost:{0}/", assemblyContext.Port));
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
