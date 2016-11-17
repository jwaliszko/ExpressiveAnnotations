using System;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class DriverFixture : IDisposable
    {
        public DriverFixture() // called before every test class
        {
            //var service = PhantomJSDriverService.CreateDefaultService();
            //service.IgnoreSslErrors = true;
            //service.WebSecurity = false;
            //Driver = new PhantomJSDriver(service, new PhantomJSOptions(), TimeSpan.FromSeconds(15)); // headless browser testing

            //var service = FirefoxDriverService.CreateDefaultService();
            //service.FirefoxBinaryPath = @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe";
            //service.HideCommandPromptWindow = true;
            //service.SuppressInitialDiagnosticInformation = true;
            //Driver = new FirefoxDriver(service, new FirefoxOptions(), TimeSpan.FromSeconds(15));
            //Driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 0, 5));

            var profile = new FirefoxProfile();
            profile.SetPreference("webdriver.log.browser.ignore", true);
            profile.SetPreference("webdriver.log.driver.ignore", true);
            profile.SetPreference("webdriver.log.profiler.ignore", true);
            profile.SetPreference("webdriver.log.init", false);
            Driver = new FirefoxDriver(profile);
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
