using System;
using System.Collections.Generic;
using System.IO;

namespace RentalBot
{
    class Settings
    {
        private Dictionary<string, dynamic> settings = new Dictionary<string, dynamic>();

        public string ChromeDriverPath
        {
            get
            {
                try
                {
                    return (string)settings["ChromeDriverPath"];
                }
                catch
                {
                    settings["ChromeDriverPath"] = "";
                    return settings["ChromeDriverPath"];
                }
            }
            set => settings["ChromeDriverPath"] = (string)value;
        }

        public string Username
        {
            get
            {
                try
                {
                    return (string)settings["Username"];
                }
                catch
                {
                    settings["Username"] = "";
                    return settings["Username"];
                }
            }
            set => settings["Username"] = (string)value;
        }

        public string WalletPWD
        {
            get
            {
                try
                {
                    return (string)settings["WalletPWD"];
                }
                catch
                {
                    settings["WalletPWD"] = "";
                    return (string)settings["WalletPWD"];
                }
            }
            set => settings["WalletPWD"] = (string)value;
        }

        public string PostKey
        {
            get
            {
                try
                {
                    return (string)settings["PostKey"];
                }
                catch
                {
                    settings["PostKey"] = "";
                    return (string)settings["PostKey"];
                }
            }
            set => settings["PostKey"] = (string)value;
        }

        public string ActiveKey
        {
            get
            {
                try
                {
                    return (string)settings["ActiveKey"];
                }
                catch
                {
                    settings["ActiveKey"] = "";
                    return (string)settings["ActiveKey"];
                }
            }
            set => settings["ActiveKey"] = (string)value;
        }

        public int MinimumCardCP
        {
            get
            {
                try
                {
                    return (int)settings["MinimumCardCP"];
                }
                catch
                {
                    settings["MinimumCardCP"] = "";
                    return (int)settings["MinimumCardCP"];
                }
            }
            set => settings["MinimumCardCP"] = (int)value;
        }

        public int TargetCP
        {
            get
            {
                try
                {
                    return (int)settings["TargetCP"];
                }
                catch
                {
                    settings["TargetCP"] = 0;
                    return (int)settings["TargetCP"];
                }
            }
            set => settings["TargetCP"] = (int)value;
        }

        public int RentalInterval
        {
            get
            {
                try
                {
                    return (int)settings["RentalInterval"];
                }
                catch
                {
                    settings["RentalInterval"] = 0;
                    return (int)settings["RentalInterval"];
                }
            }
            set => settings["RentalInterval"] = (int)value;
        }

        public int MinimumDEC
        {
            get
            {
                try
                {
                    return (int)settings["MinimumDEC"];
                }
                catch
                {
                    settings["MinimumDEC"] = 0;
                    return (int)settings["MinimumDEC"];
                }
            }
            set => settings["MinimumDEC"] = (int)value;
        }

        public Settings(string path)
        {
            ChromeDriverPath = "";

            Username = "";
            WalletPWD = "";
            PostKey = "";
            ActiveKey = "";

            TargetCP = 0;
            MinimumCardCP = 300;
            RentalInterval = 20000;
            MinimumDEC = 0;

            settings = new Dictionary<string, dynamic>();
            List<string> lines = new List<string>();

            try
            {
                lines = new List<string>(File.ReadAllLines(path));
            }
            catch (Exception e)
            {
                Console.WriteLine("Error opening file");
                Console.WriteLine(e);
            }

            foreach (string line in lines)
            {
                if (line.Length > 0 && line[0] != '#')
                {
                    string[] tokens = line.Split("=");
                    if (tokens[0].Contains("CP") || tokens[0].Contains("Interval") || tokens[0].Contains("DEC"))
                    {
                        settings[tokens[0]] = Int32.Parse(tokens[1]);
                    }
                    else
                    {
                        settings[tokens[0]] = tokens[1];
                    }
                }
            }
        }
        
        public bool Verify()
        {
            if (ChromeDriverPath == "")
            {
                Console.WriteLine("No ChromeDriverPath set, using app directory");
                ChromeDriverPath = "./";
            }

            if (Username == "")
            {
                Console.WriteLine("No Username set, Quitting");
                return false;
            }

            if (WalletPWD == "")
            {
                Console.WriteLine("No WalletPWD set, using default");
                WalletPWD = "Password1";
            }

            if (PostKey == "")
            {
                Console.WriteLine("No PostKey set, Quitting");
                return false;
            }

            if (ActiveKey == "")
            {
                Console.WriteLine("No ActiveKey set, Quitting");
                return false;
            }

            if (TargetCP == 0)
            {
                Console.WriteLine("No TargetCP set, Quitting");
                return false;
            }

            if(RentalInterval < 20000)
            {
                Console.WriteLine("Too low interval set, using default minimum");
                RentalInterval = 20000;
            }

            return true;
        }
    }
}
