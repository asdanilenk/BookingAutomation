using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingCommon
{
    public class WebdriverUtils
    {
        public static TimeSpan DefaultWait = new TimeSpan(0, 0, 20);

        public static IWebDriver CreateDriver()
        {
            IWebDriver webDriver = new ChromeDriver();

            webDriver.Manage().Timeouts().ImplicitWait = DefaultWait;
            webDriver.Manage().Window.Maximize();

            return webDriver;
        }

        public static void WaitForLoad(IWebDriver driver, int timeoutSec = 20)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeoutSec));
            wait.Until(wd =>
            {
                String status = (String)((IJavaScriptExecutor)wd).ExecuteScript("return document.readyState");
                return status == "complete";
            });
        }
    }
}
