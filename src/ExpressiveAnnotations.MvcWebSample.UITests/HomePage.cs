using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace ExpressiveAnnotations.MvcWebSample.UITests
{
    public class HomePage
    {
        private readonly RemoteWebDriver _driver;

        public HomePage(RemoteWebDriver driver)
        {
            _driver = driver;
        }

        public void Load(string url)
        {
            _driver.Navigate().GoToUrl(url);
            _driver.Manage().Window.Maximize();
        }

        public void Clean()
        {
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl("about:blank");
        }
        public void SetMode(string mode) // client, server
        {
            var elem = _driver.FindElementByXPath(string.Format("//a[@href='/System/SetValidation?type={0}&returnUrl=%2F']", mode));
            elem.Click();
        }

        public void SetLang(string code) // en, pl
        {
            var elem = _driver.FindElementByXPath(string.Format("//a[@href='/System/SetCulture?lang={0}&returnUrl=%2F']", code));
            elem.Click();
        }

        public void Submit()
        {
            var elem = _driver.FindElementByXPath("//input[@type='submit']");
            elem.Click();
        }

        public void ClickCheckbox(string id)
        {
            var elem = _driver.FindElementByXPath(string.Format("//input[@id='{0}']", id));
            elem.Click();
        }

        public void ClickCheckbox(string name, string value)
        {
            var elem = _driver.FindElementByXPath(string.Format("//input[@name='{0}'][@value='{1}']", name, value));
            elem.Click();
        }

        public void ClickRadio(string id, string value)
        {
            var elem = _driver.FindElementByXPath(string.Format("//input[@id='{0}'][@value='{1}']", id, value));
            elem.Click();
        }

        public void ClickTrigger(string trigger) // change, paste, keyup
        {
            var elem = _driver.FindElementByXPath(string.Format("//input[@id='{0}Trigger']", trigger));
            elem.Click();
            WaitForAjax();
        }

        public void Select(string id, string text)
        {
            var elem = _driver.FindElementByXPath(string.Format("//select[@id='{0}']", id));
            var select = new SelectElement(elem);
            select.SelectByText(text);
        }

        public void WriteTextarea(string id, string text)
        {
            var elem = _driver.FindElementByXPath(string.Format("//textarea[@id='{0}']", id));
            elem.SendKeys(text);
        }

        public void ClearTextarea(string id)
        {
            var elem = _driver.FindElementByXPath(string.Format("//textarea[@id='{0}']", id));
            elem.Clear();
        }

        public void WriteInput(string id, string text)
        {
            var elem = _driver.FindElementByXPath(string.Format("//input[@id='{0}']", id));
            elem.SendKeys(text);
        }

        public void ClearInput(string id)
        {
            var elem = _driver.FindElementByXPath(string.Format("//input[@id='{0}']", id));
            elem.Clear();
        }

        public string GetErrorMessage(string id)
        {
            var elem = _driver.FindElementByXPath(string.Format("//span[@data-valmsg-for='{0}']", id));
            var generated = elem.FindElements(By.TagName("span"));
            return generated.Any() ? generated.Single().Text : elem.Text;
        }

        public int GetPostbacksCount()
        {
            var elem = _driver.FindElementByXPath("//meta[@name='postbacks']");
            return int.Parse(elem.GetAttribute("content"));
        }

        private void WaitForAjax() // wait for all ajax requests initiated by jQuery to complete
        {
            while (true)
            {
                var ajaxIsComplete = (bool)(_driver as IJavaScriptExecutor).ExecuteScript("return jQuery.active == 0"); // $.active returns the number of active Ajax requests                    
                if (ajaxIsComplete)
                    break;
                Thread.Sleep(100);
            }
        }
    }
}
