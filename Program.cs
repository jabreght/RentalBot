using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RentalBot
{
    class Program
    {
        private static IWebDriver driver;
        private static WebDriverWait wait;
        private static ChromeOptions options;
        private static ChromeDriverService service;
        private static Settings settings;


        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting bot");

            LoadOptions();

            Console.WriteLine($"Minimum Card CP to look for: {settings.MinimumCardCP}");
            Console.WriteLine($"Target CP to rent: {settings.TargetCP}");

            using (driver = new ChromeDriver(service, options))
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                await GetCollectionPower(settings.Username);

                SetupWallet();
                LoadRentals();
                RentalLoop();

            }

            Console.WriteLine("Ending bot");
        }

        private static async Task GetCollectionPower(string user)
        {
            string url = "https://api2.splinterlands.com/players/details?name=" + user;
            try
            {
                string response = await client.GetStringAsync(url);
                dynamic parsedResponse = JObject.Parse(response);
                Console.WriteLine($"{settings.Username}'s collection power: {parsedResponse.collection_power}");
                //Console.WriteLine(response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.ReadLine();
        }

        private static void LoadOptions()
        {
            settings = new Settings("./Settings.cfg");

            service = ChromeDriverService.CreateDefaultService(settings.ChromeDriverPath);
            service.SuppressInitialDiagnosticInformation = true;

            options = new ChromeOptions();
            options.AddExcludedArgument("enable-logging");

            // Import HiveWallet Chrome Extension
            //options.AddArgument("--headless");
            options.AddExtension("C:\\Splinterlands\\Scraper\\HIVE Extension\\1.14.2_0.crx");
            options.AddArgument("--start-maximized");

            //If using separate non-temp profiles
            //options.AddArgument("--user-data-dir=C:\\Splinterlands\\Profiles");
            //options.AddArgument("--profile-directory=Bot");
        }

        private static void SetupWallet()
        {
            Console.WriteLine("Setting up wallet");

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
            Console.WriteLine("Loading rental page");

            // Navigate to rental page
            driver.Navigate().GoToUrl("https://peakmonsters.com/rentals");


            // Clear first popup
            FindElementDisplayedAndEnabled(".is-visible svg").Click();

            // Begin login process
            driver.FindElement(By.LinkText("Login")).Click();
            FindElementDisplayedAndEnabled(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control").Click();
            FindElementDisplayedAndEnabled(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control").SendKeys(settings.Username);
            {
                //Wait for second window handle to appear to change focused window
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
            string line = "";
            Console.WriteLine(
                    "1. CssSelector\n" +
                    "2. Frame List\n" +
                    "3. Window List\n" +
                    "4. Type END to quit");

            while ((line = Console.ReadLine()).ToUpper() != "END")
            {
                switch (line)
                {
                    case "1":
                        try
                        {
                            line = "";
                            line = Console.ReadLine();
                            IWebElement element = driver.FindElement(By.CssSelector(line));
                            Console.WriteLine($"Found element: {element}");
                            try
                            {
                                Console.WriteLine(
                                    "Which command would you like to try?\n" +
                                    "0. None\n" +
                                    "1. Click");
                                line = Console.ReadLine();
                                if (line == "1")
                                {
                                    element.Click();
                                }
                                else if (line == "2")
                                {
                                    Console.WriteLine($"Element text: {element.Text}");
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
                        IList<IWebElement> elements = driver.FindElements(By.TagName("frame"));
                        //System.out.println("Number of frames in a page :" + ele.size());
                        foreach (IWebElement ele in elements)
                        {
                            //Returns the Id of a frame.
                            Console.WriteLine("Frame Id :" + ele.GetAttribute("id"));
                            //Returns the Name of a frame.
                            Console.WriteLine("Frame name :" + ele.GetAttribute("name"));
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
                    default:
                    case "4":
                        break;
                }


                Console.WriteLine(
                    "1. CssSelector\n" +
                    "2. Frame List\n" +
                    "3. Window List\n" +
                    "4. Type END to quit");
            }

            int currentInterval = 0;
            int interval = 10000;
            DateTime startTime = DateTime.Now.AddSeconds(-10000);

            while (true && currentInterval < 10)
            {
                // Implement time based check
                // Check if x time has passed, if true perform actions, if false do nothing/logging

                if ((DateTime.Now - startTime).TotalMilliseconds >= interval)
                {
                    Console.WriteLine($"Refreshing list: {currentInterval}");

                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("window.scrollTo(0, -1000)");
                    FindElementDisplayedAndEnabled("button[class='btn btn-xs bg-teal']").Click();

                    IList<IWebElement> listings = FindElementsDisplayedAndEnabled("tbody");
                    IList<IWebElement> listingsAboveThreshhold = new List<IWebElement>();

                    foreach (IWebElement row in listings)
                    {
                        IList<IWebElement> cells = row.FindElements(By.CssSelector("td"));
                        string cpString = cells.ElementAt(12).Text;
                        cpString = cpString.Substring(0, cpString.Length - 7);
                        double cpValue;
                        Double.TryParse(cpString, out cpValue);

                        if (cpValue >= settings.MinimumCardCP)
                        {
                            listingsAboveThreshhold.Add(row);
                            Console.WriteLine($"Found card: Cost is {cpValue}");
                        }
                        else
                        {
                            break;
                        }
                        //Console.WriteLine($"Card {cells.ElementAt(0).FindElement(By.CssSelector("a")).Text}: costs {cells.ElementAt(12).Text}");
                    }
                    foreach (IWebElement row in listingsAboveThreshhold)
                    {
                        IList<IWebElement> cells = row.FindElements(By.CssSelector("td"));
                        string cpString = cells.ElementAt(12).Text;
                        cpString = cpString.Substring(0, cpString.Length - 7);
                        double cpValue;
                        Double.TryParse(cpString, out cpValue);
                        Console.WriteLine($"Added card: Cost is {cpValue}");
                        cells.ElementAt(13).Click();
                    }

                    startTime = DateTime.Now;
                    currentInterval += 1;
                }
                //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            }
        }

        private static IWebElement FindElementDisplayedAndEnabled(string css, int seconds = 20)
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
                    Console.WriteLine($"Error finding: {css}");
                    Console.WriteLine($"Error: {e}");
                    return null;
                }
            });
        }

        private static IList<IWebElement> FindElementsDisplayedAndEnabled(string css, int seconds = 20)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(seconds));

            return wait.Until(conditions =>
            {
                try
                {
                    IList<IWebElement> elements = conditions.FindElements(By.TagName(css));
                    if (elements.Count > 0) return elements[0].Displayed && elements[0].Enabled ? elements : null;
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error finding: {css}");
                    Console.WriteLine($"Error: {e}");
                    return null;
                }
            });
        }
    }
}
