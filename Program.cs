using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace RentalBot
{
    class Program
    {
        private static IWebDriver driver;
        private static WebDriverWait wait;
        private static ChromeOptions options;

        private static string username = "***REMOVED***";
        private static string pwd = "***REMOVED***";
        private static string postKey = "***REMOVED***";
        private static string activeKey = "***REMOVED***";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting bot");

            options = new ChromeOptions();
            // Import HiveWallet Chrome Extension
            options.AddExtension("C:\\Splinterlands\\Scraper\\HIVE Extension\\1.14.2_0.crx");
            //If using separate non-temp profiles
            //options.AddArgument("--user-data-dir=C:\\Splinterlands\\Profiles");
            //options.AddArgument("--profile-directory=Bot");

            using (driver = new ChromeDriver("C:\\Splinterlands", options))
            {
                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                SetupWallet();
                LoadRentals();

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
                                    else if(line == "2")
                                    {
                                        Console.WriteLine($"Element text: {element.Text}");
                                    }
                                    else if(line == "3")
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
                            return;
                    }


                    Console.WriteLine(
                        "1. CssSelector\n" +
                        "2. Frame List\n" +
                        "3. Window List\n" +
                        "4. Type END to quit");
                }
            }

            Console.WriteLine("Ending bot");
            Thread.Sleep(3000);
        }
        static void SetupWallet()
        {
            Console.WriteLine("Setting up wallet");

            // Navigate to in-browser html page for extension
            driver.Navigate().GoToUrl("chrome-extension://jcacnejopjdphbnjgfaaobbfafkihpep/html/popup.html");

            // Setup wallet password
            FindElementDisplayedAndEnabled("input[id='master_pwd']").SendKeys(pwd);
            FindElementDisplayedAndEnabled("input[id='confirm_master_pwd']").SendKeys(pwd);
            FindElementDisplayedAndEnabled("button[id='submit_master_pwd']").Click();

            // Add account username and key(posting key)
            FindElementDisplayedAndEnabled("button[id='add_by_keys']").Click();
            FindElementDisplayedAndEnabled("input[id='username']").SendKeys(username);
            FindElementDisplayedAndEnabled("input[id='pwd']").SendKeys(postKey);
            FindElementDisplayedAndEnabled("button[id='check_add_account']").Click();

            // Add key(active key)

            //wait.Until(conditions => conditions.FindElement(By.CssSelector("img[id='settings']")).Displayed);

            FindElementDisplayedAndEnabled("img[id='settings']").Click();
            FindElementDisplayedAndEnabled("div[id='manage']").Click();

            //wait.Until(conditions => conditions.FindElement(By.CssSelector("span[id='active_key_title']")).Displayed);

            FindElementDisplayedAndEnabled("div[style='display: block;']").Click();
            FindElementDisplayedAndEnabled("input[id='new_key']").SendKeys(activeKey);
            FindElementDisplayedAndEnabled("button[id='add_new_key']").Click();
        }

        static void LoadRentals()
        {
            Console.WriteLine("Loading rental page");

            // Navigate to rental page
            driver.Navigate().GoToUrl("https://peakmonsters.com/rentals");

            // Clear first popup
            FindElementDisplayedAndEnabled(".is-visible svg").Click();

            // Begin login process
            driver.FindElement(By.LinkText("Login")).Click();
            FindElementDisplayedAndEnabled(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control").Click();
            FindElementDisplayedAndEnabled(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control").SendKeys(username);
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
    }
}
