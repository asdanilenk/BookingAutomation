using BookingCommon;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Yellowstone
{
    class Program
    {
        private class Dates
        {
            public string arrival;
            public string departure;

            public Dates(string arrival, string departure)
            {
                this.arrival = arrival;
                this.departure = departure;
            }
        }

        static Dictionary<String, String> hotels = new Dictionary<string, string> {
             { "CL" , "Canyon Lodge" } ,
             { "GV" , "Grant Village" } ,
             { "LH" , "Lake Hotel and Cabins" } ,
             { "LL" , "Lake Lodge" } ,
             { "MH" , "Mammoth Hotel and Cabins" } ,
             { "OI" , "Old Faithful Inn" } ,
             { "OL" , "Old Faithful Lodge" } ,
             { "OS" , "Old Faithful Snow Lodge" } ,
             { "RL" , "Roosevelt Lodge" }
        };

        static void Main(string[] args)
        {
            var webDriver = WebdriverUtils.CreateDriver();

            List<String> rooms = new List<String>();
            Logger log = new Logger(nameof(Yellowstone));
           
            try
            {
                var DatesList = ConfigurationManager.AppSettings["dates"].Split(';')
                    .Select(i =>
                    {
                        var parts = i.Split(',');
                        return new Dates(parts[0], parts[1]);
                    });

                webDriver.Navigate().GoToUrl("http://www.yellowstonenationalparklodges.com");
                webDriver.FindElement(By.Id("book")).Click();
                foreach (var date in DatesList)
                {
                    foreach (var hotel in hotels)
                    {
                        var select = new SelectElement(webDriver.FindElement(By.Id("header-sn-location")));
                        select.SelectByValue(hotel.Key);

                        var arrival = webDriver.FindElement(By.Id("header-sn-arrival"));
                        arrival.Clear();
                        arrival.SendKeys(date.arrival);

                        var departure = webDriver.FindElement(By.Id("header-sn-departure"));
                        departure.Clear();
                        departure.SendKeys(date.departure);

                        var adults = new SelectElement(webDriver.FindElement(By.Id("header-adults")));
                        adults.SelectByValue(ConfigurationManager.AppSettings["adults"]);

                        webDriver.FindElement(By.ClassName("book-submit")).Click();

                        (new WebDriverWait(webDriver, WebdriverUtils.DefaultWait))
                            .Until((driver) => driver.WindowHandles.Count > 1);
                        webDriver.SwitchTo().Window(webDriver.WindowHandles[1]);

                        WebdriverUtils.WaitForLoad(webDriver);
                        Thread.Sleep(1000);

                        while (webDriver.FindElement(By.TagName("body")).Text == "The service is unavailable.")
                        {
                            Thread.Sleep(5000);
                            webDriver.Navigate().Refresh();
                            WebdriverUtils.WaitForLoad(webDriver);
                            Thread.Sleep(1000);
                        }

                        var prices = webDriver.FindElements(By.CssSelector("[id^=avr]"));
                        foreach (var price in prices)
                        {
                            if (!String.IsNullOrWhiteSpace(price.Text))
                            {
                                var p = int.Parse(Regex.Match(price.Text, @"\$(\d*).*").Groups[1].Value);
                                String room = $"Hotel {hotel.Value} has {price.Text} available for {date.arrival}-{date.departure}";
                                log.WriteLine(room);
                                if (p <= int.Parse(ConfigurationManager.AppSettings["maxPrice"]))
                                {
                                    rooms.Add(room);
                                }
                            }
                        }
                        webDriver.Close();
                        webDriver.SwitchTo().Window(webDriver.WindowHandles[0]);
                    }
                }
            }
            catch (Exception e)
            {
                String ex = e.Message + Environment.NewLine + e.StackTrace;
                log.WriteLine(ex);
            }
            finally
            {
                log.Close();
                webDriver.Quit();
                if (rooms.Count > 0)
                {
                    String body = String.Join(Environment.NewLine, rooms.ToArray());
                    EmailClient.SendEmail(body);
                }
            }
        }
    }
}
