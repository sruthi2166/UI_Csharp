using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;

namespace ProjectMyntra.Pages
{
    public class CheckoutPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        // multiple possible selectors for cart items
        private readonly By[] CartItemSelectors = new By[]
        {
            By.CssSelector("div.cart-item"),
            By.CssSelector("li.cart-item"),
            By.CssSelector(".itemContainer"),
            By.CssSelector(".cart-list .item"),
            By.CssSelector(".cartCard"),
            By.CssSelector("ul.cart-items li"),
            By.CssSelector(".product-item"),
            By.CssSelector(".item"),
        };

        public CheckoutPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        }

        /// <summary>
        /// Returns cart count immediately (no wait)
        /// </summary>
        public int GetCartItemCountImmediate()
        {
            foreach (var sel in CartItemSelectors)
            {
                var items = _driver.FindElements(sel);
                if (items.Any())
                    return items.Count;
            }
            return 0;
        }

        /// <summary>
        /// Waits until cart has items or timeout expires
        /// </summary>
        public int WaitForCartItems(int timeoutSeconds = 15)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                wait.Until(d =>
                {
                    foreach (var sel in CartItemSelectors)
                    {
                        var items = d.FindElements(sel);
                        if (items.Any()) return true;
                    }
                    return false;
                });
            }
            catch (WebDriverTimeoutException)
            {
                // Continue; will return 0
            }

            return GetCartItemCountImmediate();
        }

        /// <summary>
        /// Old method name for test compatibility
        /// </summary>
        public int GetCartItemCount()
        {
            return WaitForCartItems(15);
        }
    }
}
