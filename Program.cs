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
        static void Main(string[] args)
        {
            string username = "***REMOVED***";
            string pwd = "***REMOVED***";
            string postKey = "***REMOVED***";
            string activeKey = "***REMOVED***";

            Console.WriteLine("Starting bot");

            ChromeOptions options = new ChromeOptions();
            //options.AddArgument("--user-data-dir=C:\\Splinterlands\\Profiles");
            //options.AddArgument("--profile-directory=Bot");
            options.AddExtension("C:\\Splinterlands\\Scraper\\HIVE Extension\\1.14.2_0.crx");

            using (IWebDriver driver = new ChromeDriver("C:\\Splinterlands", options))
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                driver.Navigate().GoToUrl("chrome-extension://jcacnejopjdphbnjgfaaobbfafkihpep/html/popup.html");

                /*IWebElement walletPWD = wait.Until<IWebElement>(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("input[id='master_pwd']"));
                        return (element.Displayed && element.Enabled) ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        return null;
                    }
                });*/
                IWebElement walletPWD = FindElementDisplayedAndEnabled(driver, "input[id='master_pwd']");
                walletPWD.SendKeys(pwd);
                driver.FindElement(By.CssSelector("input[id='confirm_master_pwd']")).SendKeys(pwd);
                driver.FindElement(By.CssSelector("button[id='submit_master_pwd']")).Click();

                IWebElement addKeysChoice = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("button[id='add_by_keys']"));
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        return null;
                    }
                });
                addKeysChoice.Click();

                IWebElement addKeyUser = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("input[id='username']"));
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        return null;
                    }
                });
                addKeyUser.SendKeys(username);
                driver.FindElement(By.CssSelector("input[id='pwd']")).SendKeys(postKey);
                driver.FindElement(By.CssSelector("button[id='check_add_account']")).Click();

                wait.Until(conditions => conditions.FindElement(By.CssSelector("img[id='settings']")).Displayed);
                driver.FindElement(By.CssSelector("img[id='settings']")).Click();
                driver.FindElement(By.CssSelector("div[id='manage']")).Click();
                wait.Until(conditions => conditions.FindElement(By.CssSelector("span[id='active_key_title']")).Displayed);
                driver.FindElement(By.CssSelector("span[id='active_key_title']")).Click();
                driver.FindElement(By.CssSelector("input[id='new_key']")).SendKeys(activeKey);
                driver.FindElement(By.CssSelector("button[id='add_new_key']")).Click();

                driver.Navigate().GoToUrl("https://peakmonsters.com/rentals");

                IWebElement closeButton = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector(".is-visible svg"));
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        Console.WriteLine("Error retrieving close button");
                        return null;
                    }
                });
                closeButton.Click();

                driver.FindElement(By.LinkText("Login")).Click();

                IWebElement usernameInput = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector(".has-feedback-left .form-control, .has-feedback-left.input-group .form-control"));
                        return element.Displayed ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        Console.WriteLine("Error retrieving username input");
                        return null;
                    }
                });

                usernameInput.Click();
                usernameInput.SendKeys(username);
                {
                    int previousWinCount = driver.WindowHandles.Count;

                    driver.FindElement(By.CssSelector("button[class='btn btn-block no-padding']")).Click();

                    bool popupLoaded = wait.Until(conditions => conditions.WindowHandles.Count == previousWinCount + 1);
                    driver.SwitchTo().Window(driver.WindowHandles.Last());
                    IWebElement element = driver.FindElement(By.CssSelector("input[id='keep']"));
                    element.Click();
                    element = driver.FindElement(By.CssSelector("button[id='proceed']"));
                    element.Click();
                }

                /*IWebElement keepCheckbox = wait.Until<IWebElement>(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("input[id='keep']"));
                        return (element.Displayed) ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        Console.WriteLine("Error retrieving username input");
                        return null;
                    }
                });*/
                //keepCheckbox.Click();

                //driver.FindElement(By.CssSelector("button[id='proceed']")).Click();

                //Thread.Sleep(2000);
                //wait.Until(webDriver => driver.FindElement(By.LinkText("Login")).Displayed);

                //Thread.Sleep(10000);
                //driver.FindElement*
                //wait.Until(driver => driver.FindElement())
                //wait.Until(webDriver => webDriver.FindElement(By.CssSelector("h3")).Displayed);

                string line = "";
                Console.WriteLine(@"1. CssSelector
2. Frame List
3. Window List
4. Type END to quit");

                while ((line = Console.ReadLine()).ToUpper() != "END")
                {
                    switch (line)
                    {
                        case "1":
                            try
                            {
                                line = Console.ReadLine();
                                IWebElement element = driver.FindElement(By.CssSelector(line));
                                Console.WriteLine($"Found element: {element}");
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
                                driver.FindElement(By.CssSelector("input[id='keep']")).Click();
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


                    Console.WriteLine(@"1. CssSelector
2. Frame List
3. Window List
4. Type END to quit");
                }
            }

            Console.WriteLine("Ending bot");
            Thread.Sleep(3000);
        }
        void SetupWallet(string username, string pwd, string postKey, string activeKey)
        {
            Console.WriteLine("Setting up wallet");

            ChromeOptions options = new ChromeOptions();
            options.AddExtension("C:\\Splinterlands\\Scraper\\HIVE Extension\\1.14.2_0.crx");

            using (IWebDriver driver = new ChromeDriver("C:\\Splinterlands", options))
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                driver.Navigate().GoToUrl("chrome-extension://jcacnejopjdphbnjgfaaobbfafkihpep/html/popup.html");

                IWebElement walletPWD = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("input[id='master_pwd']"));
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        return null;
                    }
                });
                walletPWD.SendKeys(pwd);
                driver.FindElement(By.CssSelector("input[id='confirm_master_pwd']")).SendKeys(pwd);
                driver.FindElement(By.CssSelector("button[id='submit_master_pwd']")).Click();

                IWebElement addKeysChoice = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("button[id='add_by_keys']"));
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        return null;
                    }
                });
                addKeysChoice.Click();

                IWebElement addKeyUser = wait.Until(conditions =>
                {
                    try
                    {
                        IWebElement element = conditions.FindElement(By.CssSelector("input[id='username']"));
                        return element.Displayed && element.Enabled ? element : null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error: {e}");
                        return null;
                    }
                });
                addKeyUser.SendKeys(username);
                driver.FindElement(By.CssSelector("input[id='pwd']")).SendKeys(postKey);
                driver.FindElement(By.CssSelector("button[id='check_add_account']")).Click();

                wait.Until(conditions => conditions.FindElement(By.CssSelector("img[id='settings']")).Displayed);
                driver.FindElement(By.CssSelector("img[id='settings']")).Click();
                driver.FindElement(By.CssSelector("div[id='manage']")).Click();
                wait.Until(conditions => conditions.FindElement(By.CssSelector("span[id='active_key_title']")).Displayed);
                driver.FindElement(By.CssSelector("span[id='active_key_title']")).Click();
                driver.FindElement(By.CssSelector("input[id='new_key']")).SendKeys(activeKey);
                driver.FindElement(By.CssSelector("button[id='add_new_key']")).Click();
            }
        }
        private static IWebElement FindElementDisplayedAndEnabled(IWebDriver driver, string css)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

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
