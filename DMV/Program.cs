using BookingCommon;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace DMV
{
    class DmvBooking
    {
        static void Main(string[] args)
        {
            var webDriver = WebdriverUtils.CreateDriver();
            List<String> appointments = new List<String>();
            Logger log = new Logger(nameof(DMV));
            
            try
            {
                webDriver.Navigate().GoToUrl("https://www.dmv.ca.gov/wasapp/foa/startDriveTest.do");
                var dmvOffices = ConfigurationManager.AppSettings["offices"].Split(',').Select(i => i.Trim().ToUpper());
                
                foreach (var office in dmvOffices)
                {
                    var officeElement = new SelectElement(webDriver.FindElement(By.Id("officeId")));
                    officeElement.SelectByText(office);

                    var typeOfTestElement = webDriver.FindElement(By.Id("DT"));
                    typeOfTestElement.Click();

                    var firstNameElement = webDriver.FindElement(By.Id("first_name"));
                    if (String.IsNullOrEmpty(firstNameElement.GetAttribute("value")))
                    {
                        firstNameElement.Clear();
                        firstNameElement.SendKeys(ConfigurationManager.AppSettings["firstName"]);

                        var lastNameElement = webDriver.FindElement(By.Id("last_name"));
                        lastNameElement.Clear();
                        lastNameElement.SendKeys(ConfigurationManager.AppSettings["lastName"]);

                        var dlNumberElement = webDriver.FindElement(By.Id("dl_number"));
                        dlNumberElement.Clear();
                        dlNumberElement.SendKeys(ConfigurationManager.AppSettings["permitNumber"]);

                        var birthMonthElement = webDriver.FindElement(By.Name("birthMonth"));
                        birthMonthElement.Clear();
                        birthMonthElement.SendKeys(ConfigurationManager.AppSettings["birthMonth"]);

                        var birthDayElement = webDriver.FindElement(By.Name("birthDay"));
                        birthDayElement.Clear();
                        birthDayElement.SendKeys(ConfigurationManager.AppSettings["birthDay"]);

                        var birthYearElement = webDriver.FindElement(By.Name("birthYear"));
                        birthYearElement.Clear();
                        birthYearElement.SendKeys(ConfigurationManager.AppSettings["birthYear"]);

                        var phone = ConfigurationManager.AppSettings["phone"];
                        var telArea = phone.Substring(0, 3);
                        var telPrefix = phone.Substring(3, 3);
                        var telSuffix = phone.Substring(6, 4);

                        var telAreaElement = webDriver.FindElement(By.Name("telArea"));
                        telAreaElement.Clear();
                        telAreaElement.SendKeys(telArea);

                        var telPrefixElement = webDriver.FindElement(By.Name("telPrefix"));
                        telPrefixElement.Clear();
                        telPrefixElement.SendKeys(telPrefix);

                        var telSuffixElement = webDriver.FindElement(By.Name("telSuffix"));
                        telSuffixElement.Clear();
                        telSuffixElement.SendKeys(telSuffix);
                    }

                    webDriver.FindElement(By.XPath("//input[@type='submit']")).Click();

                    WebdriverUtils.WaitForLoad(webDriver);
                    Thread.Sleep(1000);

                    var rows = webDriver.FindElements(By.XPath("//table[contains(@class, 'table-condensed')]/tbody/tr"));

                    foreach (var row in rows)
                    {
                        var appointmentText = row.FindElement(By.XPath(".//td[@data-title=\"Appointment\"]")).Text;
                        const string text = "Sorry, this type of service is not available at this location. Please select another office.";
                        if (!appointmentText.Equals(text))
                        {
                            var dateString = Regex.Match(appointmentText, @"\w+\s*\d{1,2},\s*\d{4}").Groups[0].Value;
                            var date = Convert.ToDateTime(dateString);

                            if ((date - DateTime.Now).TotalDays < int.Parse(ConfigurationManager.AppSettings["daysInAdvance"]))
                            {
                                var location = row.FindElement(By.XPath(".//td[@data-title=\"Office\"]")).Text;
                                var appointment = $"{location}: {appointmentText}\n\n";
                                log.WriteLine(appointment);
                                appointments.Add(appointment);
                            }
                        }
                    }

                    var changeInfoButton = webDriver.FindElement(By.XPath("//input[@value=\"Change Info\"]"));
                    changeInfoButton.Click();
                }
                webDriver.Close();
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
                if (appointments.Count > 0)
                {
                    String body = String.Join(Environment.NewLine, appointments.ToArray());
                    EmailClient.SendEmail(body);
                }
            }
        }

       
    }
}
