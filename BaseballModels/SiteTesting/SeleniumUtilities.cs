using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SiteTesting
{
    internal class SeleniumUtilities
    {
        public static bool WaitForPageLoad(IWebDriver driver, string page)
        {
            driver.Navigate().GoToUrl(page);
            try {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(2));
                wait.Until(d =>
                {
                    var readyState = ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").ToString();
                    return readyState.Equals("complete", StringComparison.OrdinalIgnoreCase);
                });

                return true;
            } catch (WebDriverTimeoutException)
            {
                return false;
            }
        }
    }
}
