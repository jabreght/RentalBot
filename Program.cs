using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace RentalBot
{
    class Program
    {
        private static IWebDriver driver;
        private static WebDriverWait wait;
        private static ChromeOptions options;
        private static ChromeDriverService service;
        private static IJavaScriptExecutor js;
        private static Settings settings;

        private static List<string> logMessages = new List<string>();
        private static string configPath = "./Config";
        private static string logPath = $"{configPath}/Log.txt";
        private static readonly HttpClient client = new HttpClient();

        private static int userCP = 0;


        static async Task Main(string[] args)
        {
            WriteLog("Starting bot");

            if (LoadOptions())
            {
                Console.Title = $"RentalBot: {settings.Username}";

                //WriteLog($"Minimum Card CP to look for: {settings.MinimumCardCP}");
                //WriteLog($"Target CP to rent: {settings.TargetCP}");

                using (driver = new ChromeDriver(service, options))
                {
                    wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    js = (IJavaScriptExecutor)driver;

                    WriteLog($"{settings.Username}'s current CP is: {userCP}");

                    await UpdateCP(settings.Username);

                    if (userCP < settings.TargetCP)
                    {
                        SetupWallet();
                        LoadRentals();
                        RentalLoop();
                    }
                    else
                    {
                        WriteLog($"Target CP of {settings.TargetCP} is already achieved");
                    }
                    
                }
            }

            WriteLog("Ending bot");

            FlushLog();
        }

        private static async Task<bool> UpdateCP(string user)
        {
            // Given a bool return value so the method can be used with wait.Until()

            // User's profile JSON data
            string url = "https://api2.splinterlands.com/players/details?name=" + user;

            // Attempt to get user's JSON data and then parse for their CP(collection_power: xyz)
            try
            {
                string response = await client.GetStringAsync(url);
                dynamic parsedResponse = JObject.Parse(response);
                userCP = parsedResponse.collection_power;

                WriteLog($"{settings.Username}'s current CP is: {userCP}");
                WriteLog($"{settings.Username}'s target CP is: {settings.TargetCP}");
                WriteLog($"Minimum Card CP to look for: {settings.MinimumCardCP}");

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        private static bool LoadOptions()
        {
            WriteLog("Loading Settings");   
            settings = new Settings($"{configPath}/Settings.ini");
            if (!settings.Verify())
            {
                return false;
            }

            service = ChromeDriverService.CreateDefaultService(settings.ChromeDriverPath);
            options = new ChromeOptions();

            // Disable logging
            service.SuppressInitialDiagnosticInformation = true;
            options.AddExcludedArgument("enable-logging");

            // Import HiveWallet Chrome Extension
            //options.AddArgument("--headless");
            options.AddExtension($"{configPath}/HIVE Extension/1.14.2_0.crx");
            options.AddArgument("--start-maximized");

            // If using separate non-temp profiles
            //options.AddArgument("--user-data-dir=C:\\Splinterlands\\Profiles");
            //options.AddArgument("--profile-directory=Bot");

            return true;
        }

        private static void SetupWallet()
        {
            WriteLog("Setting up wallet");

            // Navigate to in-browser html page for extension
            driver.Navigate().GoToUrl("chrome-extension://jcacnejopjdphbnjgfaaobbfafkihpep/html/popup.html");

            // Setup wallet password
            FindElementDisplayedAndEnabled("input[id='master_pwd']").SendKeys(settings.WalletPWD);
            FindElementDisplayedAndEnabled("input[id='confirm_master_pwd']").SendKeys(settings.WalletPWD);
            FindElementDisplayedAndEnabled("button[id='submit_master_pwd']").Click();

            // Add account username and key(posting key)
            FindElementDisplayedAndEnabled("button[id='add_by_keys']").Click();
            FindElementDisplayedAndEnabled("input[id='username']").SendKeys(settings.Username);
            FindElementDisplayedAndEnabled("input[id='pwd']").SendKeys(settings.PostKey);
            FindElementDisplayedAndEnabled("button[id='check_add_account']").Click();

            // Add key(active key)
            FindElementDisplayedAndEnabled("img[id='settings']").Click();
            FindElementDisplayedAndEnabled("div[id='manage']").Click();
            FindElementDisplayedAndEnabled("div[style='display: block;']").Click();
            FindElementDisplayedAndEnabled("input[id='new_key']").SendKeys(settings.ActiveKey);
            FindElementDisplayedAndEnabled("button[id='add_new_key']").Click();
        }

        private static void LoadRentals()
        {
            WriteLog("Loading rental page");

            // Navigate to rental page
            driver.Navigate().GoToUrl("https://peakmonsters.com/rentals");

            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

            // Clear first popup
            FindElementDisplayedAndEnabled("button[class='btn btn-default btn-dark btn-sm ml-20']").Click();

            // Begin login process
            driver.FindElement(By.LinkText("Login")).Click();
            FindElementDisplayedAndEnabled(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control").Click();
            FindElementDisplayedAndEnabled(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control").SendKeys(settings.Username);
            {
                // Wait for second window handle to appear to change focused window
                int previousWinCount = driver.WindowHandles.Count;
                FindElementDisplayedAndEnabled("button[class='btn btn-block no-padding']").Click();
                bool popupLoaded = wait.Until(conditions => conditions.WindowHandles.Count == previousWinCount + 1);
                driver.SwitchTo().Window(driver.WindowHandles.Last());
                // Confirm wallet transaction and "Do Not Show Again" prompt
                driver.FindElement(By.CssSelector("input[id='keep']")).Click();
                driver.FindElement(By.CssSelector("button[id='proceed']")).Click();
                driver.SwitchTo().Window(driver.WindowHandles.Last());
            }
        }

        private static void RentalLoop()
        {
            int currentInterval = 0;
            int cardCheckInterval = 3000;
            DateTime startTime = DateTime.Now.AddSeconds(-10000);
            DateTime cardCheckTime = DateTime.Now;

            while (userCP < settings.TargetCP)
            {
                try
                {
                    if ((DateTime.Now - startTime).TotalMilliseconds >= settings.RentalInterval)
                    {
                        WriteLog($"Refreshing list: {currentInterval}");

                        try
                        {
                            js.ExecuteScript("window.scrollTo(0, -1000)");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Scroll error: {e}");
                        }

                        // Find/Click - Refresh Button
                        FindElementDisplayedAndEnabled("button[class='btn btn-xs bg-teal']").Click();

                        if (FindElementsDisplayedAndEnabledTag("tbody") != null)
                        {
                            IList<IWebElement> listings = FindElementsDisplayedAndEnabledTag("tbody");

                            foreach (IWebElement row in listings)
                            {
                                IList<IWebElement> cells = row.FindElements(By.CssSelector("td"));
                                string cpString = cells.ElementAt(12).Text;
                                cpString = cpString.Substring(0, cpString.Length - 7);
                                double cpValue;
                                Double.TryParse(cpString, out cpValue);

                                if (cpValue >= settings.MinimumCardCP)
                                {
                                    WriteLog($"Found card: {cells.ElementAt(1).Text}");
                                    WriteLog($"Cost is {cpValue}");
                                    //WriteLog($"Added card: Cost is {cpValue}");
                                    cells.ElementAt(13).Click();
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        startTime = DateTime.Now;
                        currentInterval += 1;
                        wait.Until(condition => (UpdateCP(settings.Username)));
                        FlushLog();
                    }

                    if ((DateTime.Now - cardCheckTime).TotalMilliseconds >= cardCheckInterval)
                    {

                        // Check for cards in cart
                        int cardsInCart = 0;
                        string sCardsInCart = "";
                        try
                        {
                            sCardsInCart = driver.FindElement(By.CssSelector("span[class='badge bg-warning-400']")).Text;
                            Int32.TryParse(sCardsInCart, out cardsInCart);
                        }
                        catch
                        {
                            Console.WriteLine("No cards found in cart");
                        }

                        if (cardsInCart > 0)
                        {
                            // Open cart dropdown
                            FindElementDisplayedAndEnabled("li:nth-child(5) > .dropdown-toggle").Click();

                            if (FindElementDisplayed("button[class='btn btn-xs bg-purple mt-5']").Enabled)
                            {
                                WriteLog($"{cardsInCart} in cart, starting transaction");

                                // Get starting window count before initiating a popup
                                int previousWinCount = driver.WindowHandles.Count;
                                FindElementDisplayedAndEnabled("button[class='btn btn-xs bg-purple mt-5']").Click();
                                bool popupLoaded = wait.Until(conditions => conditions.WindowHandles.Count == previousWinCount + 1);
                                driver.SwitchTo().Window(driver.WindowHandles.Last());

                                // Confirm transaction
                                FindElementDisplayed("button[id='proceed']").Click();
                                FindElementDisplayed("button[id='error-ok']").Click();

                                // Give focus back to main window
                                driver.SwitchTo().Window(driver.WindowHandles.Last());

                            }
                            else
                            {
                                WriteLog("Old cards, clearing cart");
                                FindElementDisplayedAndEnabled(".icons-list:nth-child(2) .icon-trash-alt").Click();
                            }

                            // Close cart dropdown
                            FindElementDisplayedAndEnabled("li:nth-child(5) > .dropdown-toggle").Click();
                        }
                        cardCheckTime = DateTime.Now;
                    }
                }
                catch (WebDriverTimeoutException e)
                {
                    WriteLog($"Error: {e}");
                    WriteLog("Navigating to main rental page");
                    driver.Navigate().GoToUrl("https://peakmonsters.com/rentals");
                    wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                    continue;
                }
            }

            WriteLog($"Target CP of {settings.TargetCP} achieved");
            WriteLog($"Current CP: {userCP}");
        }

        private static IWebElement FindElementDisplayed(
            string css,
            int seconds = 20,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            return wait.Until(conditions =>
            {
                try
                {
                    IWebElement element = conditions.FindElement(By.CssSelector(css));
                    return element.Displayed ? element : null;
                }
                /*catch (WebDriverTimeoutException e)
                {
                    WriteLog($"Can't find {css}");
                    WriteLog($"Error:{memberName}({sourceLineNumber})");
                    //Console.WriteLine($"Error: {e}");
                    FlushLog();


                    // Possibly build try-catch into rental loop and have it continue there instead


                    return null;
                }*/
                catch (Exception e)
                {
                    WriteLog($"Can't find {css}");
                    WriteLog($"Error:{memberName}({sourceLineNumber})");
                    //Console.WriteLine($"Error: {e}");
                    FlushLog();
                    return null;
                }
            });
        }

        private static IWebElement FindElementDisplayedAndEnabled(
            string css,
            int seconds = 20,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            return wait.Until(conditions =>
            {
                try
                {
                    IWebElement element = conditions.FindElement(By.CssSelector(css));
                    return element.Displayed && element.Enabled ? element : null;
                }
                catch (Exception e)
                {
                    WriteLog($"Can't find {css}");
                    WriteLog($"Error:{memberName}({sourceLineNumber})");
                    //Console.WriteLine($"Error: {e}");
                    FlushLog();
                    return null;
                }
            });
        }

        private static IList<IWebElement> FindElementsDisplayedAndEnabledCss(
            string css,
            int seconds = 20,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            return wait.Until(conditions =>
            {
                try
                {
                    IList<IWebElement> elements = conditions.FindElements(By.CssSelector(css));
                    if (elements.Count > 0 && elements[0].Displayed && elements[0].Enabled)
                    {
                        return elements;
                    }
                }
                catch (Exception e)
                {
                    WriteLog($"Can't find {css}");
                    WriteLog($"Error:{memberName}({sourceLineNumber})");
                    //Console.WriteLine($"Error: {e}");
                    FlushLog();
                }
                return null;
            });
        }

        private static IList<IWebElement> FindElementsDisplayedAndEnabledTag(
            string css,
            int seconds = 20,
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            return wait.Until(conditions =>
            {
                try
                {
                    IList<IWebElement> elements = conditions.FindElements(By.TagName(css));
                    if (elements.Count > 0 && elements[0].Displayed && elements[0].Enabled)
                    {
                        return elements;
                    }
                }
                catch (Exception e)
                {
                    WriteLog($"Can't find {css}");
                    WriteLog($"Error:{memberName}({sourceLineNumber})");
                    //Console.WriteLine($"Error: {e}");
                    FlushLog();
                }
                return null;
            });
        }

        private static bool LiveTesting(IWebElement webElement = null)
        {
            string choices =
                    "1. CssSelector\n" +
                    "2. CssSelector(Custom WebElement)\n" +
                    "3. Window List\n" +
                    "4. Frame List\n" +
                    "Type END to quit";
            Console.WriteLine(choices);

            string line = "";

            while ((line = Console.ReadLine()).ToUpper() != "END")
            {
                switch (line)
                {
                    case "1":
                        try
                        {
                            Console.Write("Css: ");
                            line = "";
                            line = Console.ReadLine();
                            IWebElement element = driver.FindElement(By.CssSelector(line));
                            Console.WriteLine($"Found element: {element}");
                            try
                            {
                                Console.WriteLine(
                                    "Which command would you like to try?\n" +
                                    "0. None\n" +
                                    "1. Click\n" +
                                    "2. Element's text\n" +
                                    "3. Element's attributes");
                                line = Console.ReadLine();

                                if (line == "1")
                                {
                                    element.Click();
                                }
                                else if (line == "2")
                                {
                                    Console.WriteLine($"Element text: {element.GetAttribute("textContent")}");
                                }
                                else if (line == "3")
                                {
                                    Console.WriteLine($"Element attributes: {element}");
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error: {e}");
                                Console.WriteLine($"Can't perform action: {line}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error: {e}");
                            Console.WriteLine($"Can't find element: {line}");
                        }
                        break;
                    case "2":
                        try
                        {
                            if (webElement == null) break;

                            Console.Write("Css: ");
                            line = "";
                            line = Console.ReadLine();
                            IWebElement element = webElement.FindElement(By.CssSelector(line));
                            Console.WriteLine($"Found element: {element}");
                            try
                            {
                                Console.WriteLine(
                                    "Which command would you like to try?\n" +
                                    "0. None\n" +
                                    "1. Click\n" +
                                    "2. Element's text\n" +
                                    "3. Element's attributes");
                                line = Console.ReadLine();

                                if (line == "1")
                                {
                                    element.Click();
                                }
                                else if (line == "2")
                                {
                                    Console.WriteLine($"Element text: {element.GetAttribute("textContent")}");
                                }
                                else if (line == "3")
                                {
                                    Console.WriteLine($"Element attributes: {element}");
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Error: {e}");
                                Console.WriteLine($"Can't perform action: {line}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error: {e}");
                            Console.WriteLine($"Can't find element: {line}");
                        }
                        break;

                    case "3":
                        IList<string> windows = driver.WindowHandles;
                        //System.out.println("Number of frames in a page :" + ele.size());
                        foreach (string win in windows)
                        {
                            //Returns the window handle
                            Console.WriteLine("Window Handle :" + win);
                        }
                        Console.WriteLine("Switch to which handle index?: ");
                        int winIndex = int.Parse(Console.ReadLine());
                        try
                        {
                            driver.SwitchTo().Window(driver.WindowHandles.Last());
                            //driver.FindElement(By.CssSelector("input[id='keep']")).Click();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Error: {e}");
                            Console.WriteLine($"Can't switch window: {line}");
                        }

                        break;

                    case "4":
                        IList<IWebElement> elements = driver.FindElements(By.TagName("frame"));
                        //System.out.println("Number of frames in a page :" + ele.size());
                        foreach (IWebElement ele in elements)
                        {
                            // Returns the Id of a frame.
                            Console.WriteLine("Frame Id :" + ele.GetAttribute("id"));
                            // Returns the Name of a frame.
                            Console.WriteLine("Frame name :" + ele.GetAttribute("name"));
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine(choices);
            }

            return false;
        }

        private static void WriteLog(string message)
        {
            Console.WriteLine($"{DateTime.Now}: {message}");
            logMessages.Add($"{DateTime.Now}: {message}");
        }

        private static void FlushLog()
        {
            using (StreamWriter file = new StreamWriter(logPath, true))
            {
                foreach (string line in logMessages)
                {
                    file.WriteLine(line);
                }
                logMessages.Clear();
            }
        }

        void Close()
        {

        }
    }
}
