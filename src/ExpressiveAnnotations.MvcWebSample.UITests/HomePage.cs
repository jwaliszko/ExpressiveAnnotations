using System;
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
            _driver.Manage().Window.Maximize();
            _driver.Navigate().GoToUrl(url);
        }

        public void Clean()
        {
            _driver.Manage().Cookies.DeleteAllCookies();
            _driver.Navigate().GoToUrl("about:blank");
        }

        public void SetMode(string mode) // client, server
        {
            var elem = _driver.FindElementByXPath($"//a[@href='/System/SetValidation?type={mode}&returnUrl=%2F']");
            SafeClick(elem);
        }

        public void SetLang(string code) // en, pl
        {
            var elem = _driver.FindElementByXPath($"//a[@href='/System/SetCulture?lang={code}&returnUrl=%2F']");
            SafeClick(elem);
        }

        public void Submit()
        {
            var elem = _driver.FindElementByXPath("//input[@type='submit']");
            SafeClick(elem);
        }

        public void ClickCheckbox(string id)
        {
            var elem = _driver.FindElementByXPath($"//input[@id='{id}']");
            SafeClick(elem);
        }

        public void ClickCheckbox(string name, string value)
        {
            var elem = _driver.FindElementByXPath($"//input[@name='{name}'][@value='{value}']");
            SafeClick(elem);
        }

        public void ClickRadio(string id, string value)
        {
            var elem = _driver.FindElementByXPath($"//input[@id='{id}'][@value='{value}']");
            SafeClick(elem);
        }

        public void ClickTrigger(string trigger) // change, paste, keyup
        {
            var elem = _driver.FindElementByXPath($"//input[@id='{trigger}Trigger']");
            SafeClick(elem);
            WaitForAjax();
        }

        public void Select(string id, string text)
        {
            var elem = _driver.FindElementByXPath($"//select[@id='{id}']");
            var select = new SelectElement(elem);
            select.SelectByText(text);
        }

        public void WriteTextarea(string id, string text)
        {
            var elem = _driver.FindElementByXPath($"//textarea[@id='{id}']");
            elem.SendKeys(text);
        }

        public void ClearTextarea(string id)
        {
            var elem = _driver.FindElementByXPath($"//textarea[@id='{id}']");
            elem.Clear();
        }

        public void WriteInput(string id, string text)
        {
            var elem = _driver.FindElementByXPath($"//input[@id='{id}']");
            elem.SendKeys(text);
        }

        public void SetDate(string id, string text) // simulation of datepicker instant date selection (instead of sending key characters, one after another)
        {
            _driver.ExecuteScript($"$('#{id}').attr('value', '{text}')");
            _driver.ExecuteScript("$('.date').trigger('change')");
        }

        public void ClearInput(string id)
        {
            var elem = _driver.FindElementByXPath($"//input[@id='{id}']");
            elem.Clear();
        }

        public string GetErrorMessage(string id)
        {
            var elem = _driver.FindElementByXPath($"//span[@data-valmsg-for='{id}']");
            var generated = elem.FindElements(By.TagName("span"));
            return generated.Any() ? generated.Single().Text : elem.Text;
        }

        public int GetPostbacksCount()
        {
            var elem = _driver.FindElementByXPath("//meta[@name='postbacks']");
            return int.Parse(elem.GetAttribute("content"));
        }

        private void SafeClick(IWebElement elem)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            wait.Until(ExpectedConditions.ElementToBeClickable(elem)).Click();
        }

        private void WaitForAjax() // wait for all ajax requests initiated by jQuery to complete
        {
            while (true)
            {
                var ajaxIsComplete = _driver.ExecuteScript("return jQuery.active == 0"); // $.active returns the number of active Ajax requests                    
                if ((bool) ajaxIsComplete)
                    break;
                Thread.Sleep(100);
            }
        }
    }
}
