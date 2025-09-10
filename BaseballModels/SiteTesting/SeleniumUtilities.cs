using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SiteTesting
{
    internal class SeleniumUtilities
    {
        public static bool WaitForPageLoad(ChromeDriver driver, string page)
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

        public static (bool, string) AnyErrors(ChromeDriver driver)
        {
            ILogs logs = driver.Manage().Logs;
            var entries = logs.GetLog(LogType.Browser);
            var errorLogs = entries.Where(f => f.Level == LogLevel.Severe).Select(f => f.Message);
            if (errorLogs.Any())
                return (true, errorLogs.First());

            return (false, "");
        }
    }
}
