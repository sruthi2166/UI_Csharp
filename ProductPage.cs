using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Linq;

namespace ProjectMyntra.Pages
{
    public class ProductPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        // try several selectors used on Myntra product pages
        private readonly By[] AddToBagSelectors = new By[]
        {
            By.CssSelector("button.pdp-add-to-bag"),
            By.CssSelector("button[data-test='add-to-bag']"),
            By.CssSelector("button.add-to-bag"),
            By.CssSelector("button[title='Add to Bag']"),
            By.XPath("//button[contains(translate(., 'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'), 'add to bag')]"),
            By.XPath("//div[normalize-space()='ADD TO BAG']"),
            By.XPath("//button[contains(., 'ADD TO BAG') or contains(., 'Add to Bag') or contains(., 'Add To Bag')]")
        };

        private By ProductTitle => By.CssSelector("h1.pdp-title, h1");
        private By GoToBagButton => By.CssSelector("a[href*='cart'], a[data-track='bag'], .cart-link, a[href*='/cart']");

        public ProductPage(IWebDriver driver, int waitSeconds = 15)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(waitSeconds));
        }

        public string GetProductTitle()
        {
            try
            {
                var el = _wait.Until(d => d.FindElement(ProductTitle));
                return el.Text.Trim();
            }
            catch { return string.Empty; }
        }

        // ⭐ NEW — Select size "S"
        public bool SelectSizeS()
        {
            try
            {
                By[] selectorsForS =
                {
                    By.XPath("//li[normalize-space()='S']"),
                    By.XPath("//button[normalize-space()='S']"),
                    By.XPath("//div[contains(@class,'size')]//button[normalize-space()='S']"),
                    By.XPath("//label[normalize-space()='S']"),
                    By.XPath("//li[contains(@class,'size')][normalize-space()='S']")
                };

                foreach (var sel in selectorsForS)
                {
                    var sizeElements = _driver.FindElements(sel);

                    if (sizeElements != null && sizeElements.Count > 0)
                    {
                        foreach (var s in sizeElements)
                        {
                            if (!s.Displayed || !s.Enabled) continue;

                            try
                            {
                                s.Click();
                                System.Threading.Thread.Sleep(300);
                                return true;
                            }
                            catch
                            {
                                try
                                {
                                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", s);
                                    System.Threading.Thread.Sleep(300);
                                    return true;
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        // ⭐ UPDATED — This method now selects size S
        public void SelectSizeAndAddToBag(int timeoutSeconds = 15)
        {
            // wait page load
            try { _wait.Until(d => d.FindElement(ProductTitle)); } catch { }

            // ⭐ first attempt to pick size S
            bool sizeSSelected = SelectSizeS();
            if (sizeSSelected)
                System.Threading.Thread.Sleep(400);

            // fallback: if S is not available, select any size
            if (!sizeSSelected)
            {
                sizeSSelected = SelectFirstAvailableSize();
                if (sizeSSelected) System.Threading.Thread.Sleep(400);
            }

            // === FIND ADD TO BAG ===
            IWebElement found = null;
            Exception lastEx = null;

            foreach (var sel in AddToBagSelectors)
            {
                try
                {
                    found = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds))
                                .Until(d =>
                                {
                                    var e = d.FindElements(sel).FirstOrDefault();
                                    return (e != null && e.Displayed && e.Enabled) ? e : null;
                                });

                    if (found != null) break;
                }
                catch (Exception ex) { lastEx = ex; }
            }

            if (found == null)
            {
                SaveDiagnostics("AddToBagNotFound");
                string msg = "Add to Bag button not found.";
                if (!sizeSSelected) msg += " Size S not selected.";
                throw new NoSuchElementException(msg, lastEx);
            }

            // === CLICK ADD TO BAG ===
            try
            {
                found.Click();
            }
            catch
            {
                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", found);
                }
                catch (Exception ex)
                {
                    SaveDiagnostics("AddToBagClickFailed");
                    throw new Exception("Failed clicking Add to Bag.", ex);
                }
            }
        }

        // fallback: first available size
        public bool SelectFirstAvailableSize()
        {
            try
            {
                By[] selectors =
                {
                    By.CssSelector("ul.size-list li"),
                    By.CssSelector("div.size-buttons li"),
                    By.CssSelector("li.size"),
                    By.CssSelector("button.size")
                };

                foreach (var sel in selectors)
                {
                    var sizes = _driver.FindElements(sel);

                    foreach (var s in sizes)
                    {
                        if (!s.Displayed || !s.Enabled) continue;

                        try
                        {
                            s.Click();
                            return true;
                        }
                        catch
                        {
                            try
                            {
                                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", s);
                                return true;
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        public void GoToBag()
        {
            try
            {
                var bag = _wait.Until(d => d.FindElement(GoToBagButton));
                bag.Click();
            }
            catch
            {
                _driver.Navigate().GoToUrl("https://www.myntra.com/cart");
            }
        }

        private void SaveDiagnostics(string prefix)
        {
            try
            {
                var reportsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\..\\..\\Reports\\Failures");
                Directory.CreateDirectory(reportsDir);

                string ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                if (_driver is ITakesScreenshot tsDriver)
                {
                    var ss = tsDriver.GetScreenshot();
                    var path = Path.Combine(reportsDir, $"{prefix}_{ts}.png");
                    ss.SaveAsFile(path);
                }

                File.WriteAllText(
                    Path.Combine(reportsDir, $"{prefix}_{ts}.html"),
                    _driver.PageSource
                );
            }
            catch { }
        }
    }
}
