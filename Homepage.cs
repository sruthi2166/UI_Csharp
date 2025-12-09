using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace ProjectMyntra.Pages
{
    public class Homepage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private By SearchInput => By.CssSelector("input.desktop-searchBar, input[type='search']");
        private By SearchSuggestionList => By.CssSelector("ul.react-autosuggest__suggestions-list, .desktop-searchBar-list");

        public Homepage(IWebDriver driver, int waitSeconds = 15)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
        }

        public void GoTo()
        {
            _driver.Navigate().GoToUrl("https://www.myntra.com/");
            _wait.Until(d => ((IJavaScriptExecutor)d)
            .ExecuteScript("return document.readyState").Equals("complete"));
        }

        public void Search(string query)
        {
            var input = _wait.Until(d => d.FindElement(SearchInput));
            input.Clear();
            input.SendKeys(query);
            input.SendKeys(Keys.Enter);
        }
    }
}

