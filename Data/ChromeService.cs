using System;
using System.ComponentModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace WebScraper.Data
{
    public class ChromeService
    {
        public string Initialize(string pageUrl)
        {
            //do not open chrome
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("headless");
            //no chrome driver console
            ChromeDriverService driverService = ChromeDriverService.CreateDefaultService();
            driverService.HideCommandPromptWindow = true;
            ChromeDriver driver = new ChromeDriver(driverService, options);
            driver.Navigate().GoToUrl(pageUrl);
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0,0,5));
            wait.Until(driver =>((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            return driver.PageSource;
        }
    }
}