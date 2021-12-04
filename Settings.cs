using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RentalBot
{
    class Settings
    {
        private string _chromeDriverPath;
        public string ChromeDriverPath
        {
            get => _chromeDriverPath;
            set => _chromeDriverPath = value;
        }

        private string _username;
        public string Username
        {
            get => _username;
            set => _username = value;
        }
        private string _walletPWD;
        public string WalletPWD
        {
            get => _walletPWD;
            set => _walletPWD = value;
        }
        private string _postKey;
        public string PostKey
        {
            get => _postKey;
            set => _postKey = value;
        }
        private string _activeKey;
        public string ActiveKey
        {
            get => _activeKey;
            set => _activeKey = value;
        }
        
        private int _minimumCardCP;
        public int MinimumCardCP
        {
            get => _minimumCardCP;
            set => _minimumCardCP = value;
        }
        private int _targetCP;
        public int TargetCP
        {
            get => _targetCP;
            set => _targetCP = value;
        }


        public Settings(string path)
        {
            FileStream configFile;

            using (configFile = new FileStream(path, FileMode.Open))
            {
                /*try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error opening file");
                    Console.WriteLine(e);
                }*/

                ChromeDriverPath = "C:\\Splinterlands";

                Username = "***REMOVED***";
                WalletPWD = "***REMOVED***";
                PostKey = "***REMOVED***";
                ActiveKey = "***REMOVED***";

                MinimumCardCP = 600;
                TargetCP = 15000;
            }
        }
    }
}
