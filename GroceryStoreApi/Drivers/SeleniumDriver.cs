using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using System.Reflection;
using System.Runtime.InteropServices;
using TechTalk.SpecFlow;

namespace Drivers
{
    public class SeleniumDriver
    {
        public IWebDriver _driver;
        public ScenarioContext _scenarioContext;

        public SeleniumDriver(ScenarioContext scenarioContext) => _scenarioContext = scenarioContext;

        public IWebDriver Setup(string browserName)
        {
            dynamic capability = GetBrowserOptions(browserName);

            //if (browserName.Contains("remote"))
            //{
            //_driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), capability.ToCapabilities());
            //}

            Environment.SetEnvironmentVariable(
                "WebDriver.gecko.driver",
                Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"Resources\geckodriver.exe")
            );

            // Set the driver
            _scenarioContext.Set(_driver, "WebDriver");

            _driver.Manage().Window.Maximize();

            return _driver;
        }

        private dynamic GetBrowserOptions(string browserName)
        {
            var driverPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Resources");

            //var driverPath = @"C:\repos\";
            //System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (browserName.ToLower().Contains("chrome") && !IsWindows)
            {
                _driver = new ChromeDriver(driverPath + "/Linux");

                return new ChromeOptions();
            }

            if (browserName.ToLower().Contains("chrome"))
            {
                ChromeOptions options = new ChromeOptions();

                //options.AddArgument("--headless");
                options.AddArgument("no-sandbox");

                _driver = new ChromeDriver(ChromeDriverService.CreateDefaultService(driverPath, "chromedriver.exe"), options, TimeSpan.FromMinutes(5));

                _driver.Manage().Timeouts().ImplicitWait.Add(TimeSpan.FromSeconds(60));
                _driver.Manage().Timeouts().PageLoad.Add(TimeSpan.FromSeconds(60));

                return options;
            }

            if (browserName.ToLower().Contains("firefox"))
            {
                _driver = new FirefoxDriver(driverPath);

                return new FirefoxOptions();
            }

            if (browserName.Contains("MicrosoftEdge"))
            {
                return new EdgeOptions();
            }

            if (browserName.Contains("Safari"))
            {
                return new SafariOptions();
            }

            // If non, return ChromeOptions
            return new ChromeOptions();
        }


    }
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv => drv.FindElement(by));
            }
            return driver.FindElement(by);
        }
    }

}
