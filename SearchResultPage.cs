using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace ProjectMyntra.Pages
{
    public class SearchResultPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        private By ProductCards => By.CssSelector("li.product-base, div.productCard, ul.results li");
        private By FirstProductLink => By.CssSelector("li.product-base a[href], div.productCard a[href]");

        public SearchResultPage(IWebDriver driver, int waitSeconds = 15)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
        }

        public int GetResultsCount()
        {
            var cards = _wait.Until(d => d.FindElements(ProductCards));
            return cards.Count;
        }

        public void OpenFirstProduct()
        {
            // capture current window handles
            var beforeHandles = _driver.WindowHandles.ToList();

            var link = _wait.Until(d => d.FindElement(FirstProductLink));
            // Prefer normal click (may open same tab), otherwise use JS
            try { link.Click(); }
            catch { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", link); }

            // wait for either same-tab navigation (URL change) or new tab appearing
            var timeout = TimeSpan.FromSeconds(10);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed < timeout)
            {
                System.Threading.Thread.Sleep(200);
                var afterHandles = _driver.WindowHandles.ToList();
                if (afterHandles.Count > beforeHandles.Count)
                {
                    // new tab opened — switch to it
                    var newHandle = afterHandles.Except(beforeHandles).First();
                    _driver.SwitchTo().Window(newHandle);
                    return;
                }

                // or if URL contains '/product' or page title changed (heuristic)
                if (!_driver.Url.Contains("/search") && !string.IsNullOrWhiteSpace(_driver.Title))
                {
                    return; // navigated in same tab
                }
            }

            // final fallback: continue on same tab
        }
    }
}
