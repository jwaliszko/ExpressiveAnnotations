using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class HomePage : IDisposable
    {
        public HomePage()
        {
            Browser = new PhantomJSDriver(); // headless browser testing
            Browser.Manage().Window.Maximize();
        }

        public RemoteWebDriver Browser { get; private set; }

        public void NavigateToSelf()
        {
            Browser.Navigate().GoToUrl("http://localhost:51622/");
        }

        public void SetClientMode()
        {
            var selector = "//a[@href='/System/SetValidation?type=client&returnUrl=%2F']";
            var lnk = Browser.FindElementByXPath(selector);
            lnk.Click();
        }

        public void SetServerMode()
        {
            var selector = "//a[@href='/System/SetValidation?type=server&returnUrl=%2F']";
            var lnk = Browser.FindElementByXPath(selector);
            lnk.Click();
        }

        public void SetPolishLang()
        {
            var selector = "//a[@href='/System/SetCulture?lang=pl&returnUrl=%2F']";
            var lnk = Browser.FindElementByXPath(selector);
            lnk.Click();
        }

        public void Submit()
        {
            var sub = Browser.FindElementByXPath("//input[@type='submit']");
            sub.Click();
        }

        public void ClickGoAbroad()
        {
            var go = Browser.FindElementByXPath("//input[@id='GoAbroad']");
            go.Click();
        }

        public void ClickChangeTrigger()
        {
            var go = Browser.FindElementByXPath("//input[@id='changeTrigger']");
            go.Click();
            WaitForAjax();
        }

        public string GetBloodTypeError()
        {
            var blood = Browser.FindElementByXPath("//span[@data-valmsg-for='BloodType']");
            var generated = blood.FindElements(By.TagName("span"));
            return generated.Any() ? generated.Single().Text : blood.Text;
        }

        public string GetSelectedMode()
        {
            var client = Browser.FindElementByXPath("//a[@href='/System/SetValidation?type=client&returnUrl=%2F']");
            var server = Browser.FindElementByXPath("//a[@href='/System/SetValidation?type=server&returnUrl=%2F']");
            if (client.Text.Contains("[") && !server.Text.Contains("]"))
                return client.Text;
            if (!client.Text.Contains("[") && server.Text.Contains("]"))
                return server.Text;

            return null;
        }

        private void WaitForAjax() // wait for all ajax requests initiated by jQuery to complete
        {
            while (true)
            {
                var ajaxIsComplete = (bool) (Browser as IJavaScriptExecutor).ExecuteScript("return jQuery.active == 0"); // $.active returns the number of active Ajax requests                    
                if (ajaxIsComplete)
                    break;
                Thread.Sleep(100);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Browser != null)
                {
                    Browser.Quit();
                    Browser = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
