using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

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

        public void SetMode(string mode) // client, server
        {
            var elem = Browser.FindElementByXPath(string.Format("//a[@href='/System/SetValidation?type={0}&returnUrl=%2F']", mode));
            elem.Click();
        }

        public void SetLang(string code) // en, pl
        {
            var elem = Browser.FindElementByXPath(string.Format("//a[@href='/System/SetCulture?lang={0}&returnUrl=%2F']", code));
            elem.Click();
        }

        public void Submit()
        {
            var elem = Browser.FindElementByXPath("//input[@type='submit']");
            elem.Click();
        }

        public void ClickCheckbox(string id)
        {
            var elem = Browser.FindElementByXPath(string.Format("//input[@id='{0}']", id));
            elem.Click();
        }

        public void ClickCheckbox(string name, string value)
        {
            var elem = Browser.FindElementByXPath(string.Format("//input[@name='{0}'][@value='{1}']", name, value));
            elem.Click();
        }

        public void ClickRadio(string id, string value)
        {
            var elem = Browser.FindElementByXPath(string.Format("//input[@id='{0}'][@value='{1}']", id, value));
            elem.Click();
        }

        public void ClickTrigger(string trigger) // change, paste, keyup
        {
            var elem = Browser.FindElementByXPath(string.Format("//input[@id='{0}Trigger']", trigger));
            elem.Click();
            WaitForAjax();
        }

        public void Select(string id, string text)
        {
            var elem = Browser.FindElementByXPath(string.Format("//select[@id='{0}']", id));
            var select = new SelectElement(elem);
            select.SelectByText(text);
        }

        public void WriteTextarea(string id, string text)
        {
            var elem = Browser.FindElementByXPath(string.Format("//textarea[@id='{0}']", id));
            elem.SendKeys(text);
        }

        public void ClearTextarea(string id)
        {
            var elem = Browser.FindElementByXPath(string.Format("//textarea[@id='{0}']", id));
            elem.Clear();
        }

        public void WriteInput(string id, string text)
        {
            var elem = Browser.FindElementByXPath(string.Format("//input[@id='{0}']", id));
            elem.SendKeys(text);
        }

        public void ClearInput(string id)
        {
            var elem = Browser.FindElementByXPath(string.Format("//input[@id='{0}']", id));
            elem.Clear();
        }

        public string GetErrorMessage(string id)
        {
            var elem = Browser.FindElementByXPath(string.Format("//span[@data-valmsg-for='{0}']", id));
            var generated = elem.FindElements(By.TagName("span"));
            return generated.Any() ? generated.Single().Text : elem.Text;
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

        public int GetPostbacksCount()
        {
            var elem = Browser.FindElementByXPath("//meta[@name='postbacks']");
            return int.Parse(elem.GetAttribute("content"));
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
