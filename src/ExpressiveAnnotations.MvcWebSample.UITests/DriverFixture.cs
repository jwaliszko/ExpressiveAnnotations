using System;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class DriverFixture : IDisposable
    {
        public DriverFixture() // called before every test class
        {
            var service = PhantomJSDriverService.CreateDefaultService();
            service.IgnoreSslErrors = true;
            service.WebSecurity = false;

            var options = new PhantomJSOptions();            

            Driver = new PhantomJSDriver(service, options, TimeSpan.FromSeconds(15)); // headless browser testing
        }

        public RemoteWebDriver Driver { get; private set; }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Driver != null)
                {
                    Driver.Quit();
                    Driver = null;
                }
            }
        }

        public void Dispose() // called after every test class
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
