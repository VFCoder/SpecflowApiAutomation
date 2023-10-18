using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Threading;

namespace Hooks
{
    public class Helpers
    {
        public static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomEmail(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string randomString = new (Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return randomString + "@email.com";
        }

        public static string RandomDigits(int length)
        {
            var random = new Random();
            string s = string.Empty;
            for (int i = 0; i < length; i++)
                s = string.Concat(s, random.Next(10).ToString());
            return s;
        }

        public static Func<IWebDriver, bool> ElementIsVisible(IWebElement element)
        {
            WaitForSpecificSeconds(2);
            return (driver) =>
            {
                try
                {
                    return element.Displayed;
                }
                catch (Exception)
                {
                    // If element is null, stale or if it cannot be located
                    return false;
                }
            };

        }

        public static string GetSourceFileId(int SourceFileLength)
        {
            string _sourceFileId = "SfId" + RandomString(SourceFileLength);
            return _sourceFileId;
        }

        public static void waitForPageToLoad(IWebDriver _driver)
        {
            IWait<IWebDriver> wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15.00));
            wait.Until(driver1 => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
        }


        public static void WaitForSpecificSeconds(int numOfSeconds)
        {
            Thread.Sleep(numOfSeconds * 1000);
        }

        public static void WaitForElementToBeClickable(IWebDriver _driver, string locator)
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath(locator)));
        }
        public static void WaitForLoading(IWebDriver _driver, int timeoutInSeconds)
        {
            new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutInSeconds)).Until(d =>
            {
                bool ajaxComplete;
                bool jsReady;
                bool loaderHidden = false;

                IJavaScriptExecutor js = (IJavaScriptExecutor)d;
                jsReady = (bool)js.ExecuteScript("return (document.readyState == \"complete\" || document.readyState == \"interactive\")"); ;

                try
                {
                    ajaxComplete = (bool)js.ExecuteScript("var result = true; try { result = (typeof jQuery != 'undefined') ? jQuery.active == 0 : true } catch (e) {}; return result;");
                }
                catch (Exception)
                {
                    ajaxComplete = true;
                }
                try
                {
                    loaderHidden = !d.FindElement(By.ClassName("Loader__background")).Displayed;
                }
                catch (Exception) { }

                return ajaxComplete && jsReady && loaderHidden;
            });

        }

    }
}
