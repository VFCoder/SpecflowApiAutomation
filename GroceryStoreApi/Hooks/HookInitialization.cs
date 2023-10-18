using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.IO;
using TechTalk.SpecFlow;
using Drivers;


//[assembly: Parallelizable(ParallelScope.Fixtures)]
//[assembly: LevelOfParallelism(2)]

namespace Hooks
{
    [Binding]
    public class HookInitialization
    {
        public readonly ScenarioContext _scenarioContext;

        public HookInitialization(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            var configuration = new TestConfiguration().GetConfiguration();
            scenarioContext.Set(configuration, "Configuration");
        }

        [BeforeScenario]
        public void BeforeScenario()
        {
            Process[] chromeDriverProcesses = Process.GetProcessesByName("chromedriver.exe");
            foreach (var chromeDriverProcess in chromeDriverProcesses)
            {
                chromeDriverProcess.Kill();
            }
            SeleniumDriver seleniumDriver = new SeleniumDriver(_scenarioContext);
            _scenarioContext.Set(seleniumDriver, "SeleniumDriver");
        }

        [AfterScenario]
        public void AfterScenario()
        {
            try
            {
                var driver = (IWebDriver)_scenarioContext["WebDriver"];
                if (_scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError)
                {
                    Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
                    string directory = @"C:\screenShots\";
                    string ssfile = $"{directory}Screen-" + DateTime.Now.ToString("yyyy-MM-dd-HH_mm_ss") + ".png";
                    if (Directory.Exists(directory))
                    {
                        ss.SaveAsFile(ssfile, ScreenshotImageFormat.Png);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            Console.WriteLine("Selenium Webdriver Quit");
            if (_scenarioContext.ContainsKey("WebDriver"))
            {
                _scenarioContext.Get<IWebDriver>("WebDriver").Close();
                _scenarioContext.Get<IWebDriver>("WebDriver").Quit();
                _scenarioContext.Get<IWebDriver>("WebDriver").Dispose();
            }
            Process[] chromeDriverProcesses = Process.GetProcessesByName("chromedriver");
            foreach (var chromeDriverProcess in chromeDriverProcesses)
            {
                chromeDriverProcess.Kill();
            }
        }
    }
}
