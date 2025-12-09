using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace ProjectMyntra.Drivers
{
    public static class DriverFactory
    {
        public static IWebDriver CreateChromeDriver(bool headless = false)
        {
            var options = new ChromeOptions();
            if (headless) options.AddArgument("--headless=new");
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-gpu");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            var driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            return driver;
        }
    }
}
