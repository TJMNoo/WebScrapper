using System;
using System.ComponentModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace WebScraper.Data
{
    public class ChromeService
    {
        //note: preferable to use selenium only for getting data, not for extracting since it is too heavy - use htmlagilitypack instead
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
            string source = driver.PageSource;
            //must quit the driver when we are done to prevent resource hog
            driver.Quit();
            return source;
        }
    }
}