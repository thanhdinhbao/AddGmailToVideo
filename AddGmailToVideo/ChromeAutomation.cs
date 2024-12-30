using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AddGmailToVideo
{
    public class ChromeAutomation
    {
        private ChromeDriver _driver;
        private WebDriverWait _wait;

        void Sleep(int time)
        {
            Thread.Sleep(time*1000);
        }

        public ChromeAutomation(string userDataPath, string profile)
        {
            InitializeDriver(userDataPath, profile);
        }


        private void InitializeDriver(string userDataPath, string profile)
        {
            string profilePath = Path.Combine(userDataPath, profile);
            ChromeDriverService cService = ChromeDriverService.CreateDefaultService();
            cService.HideCommandPromptWindow = true;

            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--window-size=1200,800");
            options.AddArguments("disable-infobars");
            options.AddArgument("-start-maximized");
            options.AddExcludedArgument("enable-automation");
            options.AddArgument($"--user-data-dir={userDataPath}");
            options.AddArgument($"--profile-directory={profile}");
            options.AddArguments(new List<string>
        {
            "no-sandbox",
            "disable-gpu"
        });

            _driver = new ChromeDriver(cService, options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        }

        public void NavigateToUrl(string url)
        {
            _driver.Navigate().GoToUrl(url);
        }

        public void ClickElement(By locator, int timeoutInSeconds = 3)
        {
            try
            {
                WebDriverWait shortWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds));
                IWebElement element = shortWait.Until(ExpectedConditions.ElementToBeClickable(locator));
                element.Click();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error clicking element: " + ex.Message);
            }
        }

        public void InputText(By locator, string text)
        {
            try
            {
                IWebElement input = _driver.FindElement(locator);
                input.SendKeys(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inputting text: " + ex.Message);
            }
        }

        public void ExecuteScript(string script, params object[] args)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript(script, args);
        }

        public void RefreshPage()
        {
            _driver.Navigate().Refresh();
        }

        public void Close()
        {
            _driver.Close();
            _driver.Quit();
        }
    }
}
