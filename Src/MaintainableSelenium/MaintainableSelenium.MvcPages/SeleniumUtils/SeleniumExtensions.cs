using System;
using System.Diagnostics.Contracts;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace MaintainableSelenium.MvcPages.SeleniumUtils
{
    public static class SeleniumExtensions
    {
        private const int SearchElementDefaultTimeout = 30;

        /// <summary>
        /// Get rid of focus from currently focused element
        /// </summary>
        /// <param name="driver">Selenium webdriver</param>
        public static void Blur(this RemoteWebDriver driver)
        {
            if(IsThereElementWithFocus(driver))
            {
                Thread.Sleep(500);
                driver.ExecuteScript("var f= document.querySelector(':focus'); if(f!=undefined){f.blur()}");
                Thread.Sleep(500);
            }
        }


        public static int GetVerticalScrollWidth(this RemoteWebDriver driver)
        {
            //INFO: It's hard to get scrollbar width using JS. 17 its default size of scrollbar on Ms Windows platform
            return 17;
        }

        public static int GetWindowHeight(this RemoteWebDriver driver)
        {
            return (int)(long)driver.ExecuteScript("return window.innerHeight");
        }

        public static int GetPageHeight(this RemoteWebDriver driver)
        {
            var scriptResult = driver.ExecuteScript("return Math.max(document.body.scrollHeight, document.body.offsetHeight, document.documentElement.clientHeight, document.documentElement.scrollHeight, document.documentElement.offsetHeight);");
            return (int)(long)scriptResult;
        }

        public static void ScrollTo(this RemoteWebDriver driver, int y)
        {
            driver.ExecuteScript(string.Format("window.scrollTo(0,{0})", y));
            Thread.Sleep(100);
        }


        /// <summary>
        /// Check if any element has currently focus
        /// </summary>
        /// <param name="driver">Selenium webdriver</param>
        [Pure]
        private static bool IsThereElementWithFocus(RemoteWebDriver driver)
        {
            try
            {
                driver.FindElementByCssSelector(":focus");
            }
            catch
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Search for element with given id
        /// </summary>
        /// <param name="driver">Selenium driver</param>
        /// <param name="elementId">Id of expected element</param>
        /// <param name="timeout">Timout for element serch</param>
        public static IWebElement GetElementById(this RemoteWebDriver driver, string elementId, int timeout = SearchElementDefaultTimeout)
        {
            try
            {
                return driver.GetElementBy(By.Id(elementId), timeout);
            }
            catch (WebDriverTimeoutException ex)
            {
                if (ex.InnerException is NoSuchElementException)
                {
                    var message = string.Format("Cannot find element with id='{0}'", elementId);
                    throw new WebElementNotFoundException(message, ex);
                }
                throw;
            }
        }

        /// <summary>
        /// Search for element using <see cref="By"/> criterion
        /// </summary>
        /// <param name="driver">Selenium driver</param>
        /// <param name="by"><see cref="By"/> criterion for given element</param>
        /// <param name="timeout">Timout for element serch</param>
        private static IWebElement GetElementBy(this RemoteWebDriver driver, By by, int timeout = SearchElementDefaultTimeout)
        {
            var waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
            var formElement = waiter.Until(ExpectedConditions.ElementIsVisible(by));
            return formElement;
        }

        private static IWebElement GetElementByInScope(RemoteWebDriver driver, By @by, IWebElement scope)
        {
            var waiter = new WebDriverWait(driver, TimeSpan.FromSeconds(SearchElementDefaultTimeout));
            var expectedElement = waiter.Until(
                (a) =>
                {
                    var element = scope.FindElement(@by);
                    if (element != null && element.Displayed && element.Enabled)
                    {
                        return element;
                    }
                    return null;
                });
            return expectedElement;
        }

        /// <summary>
        /// Return parent of given web element
        /// </summary>
        /// <param name="node">Child element</param>
        public static IWebElement GetParent(this IWebElement node)
        {
            return node.FindElement(By.XPath(".."));
        }

        /// <summary>
        /// Return type of input represented by the given web element
        /// </summary>
        /// <param name="inputElement">Web element</param>
        public static string GetInputType(this IWebElement inputElement)
        {
            var inputType = inputElement.GetAttribute("type") ?? string.Empty;
            return inputType.ToLower();
        }

        /// <summary>
        /// Click on any element with given text
        /// </summary>
        /// <param name="driver">Selenium driver</param>
        /// <param name="scope">Scope element to narrow link search</param>
        /// <param name="linkText">Element tekst</param>
        public static  void ClickOnElementWithText(this RemoteWebDriver driver, IWebElement scope, string linkText)
        {
            try
            {
                var by = By.XPath(string.Format(".//*[contains(text(), '{0}') or (@type='submit' and @value='{0}')]", linkText));
                var expectedElement = GetElementByInScope(driver, @by, scope);
                expectedElement.Click();
            }
            catch (WebDriverTimeoutException ex)
            {
                if (ex.InnerException is NoSuchElementException)
                {
                    var message = string.Format("Cannot find element with text='{0}'", linkText);
                    throw new WebElementNotFoundException(message, ex);
                }
                throw;
            }
        }
    }
}