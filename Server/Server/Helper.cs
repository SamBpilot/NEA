using System.Net.Sockets;
using System.Net;
using System.Collections;

namespace NEA
{
    /// <summary>
    /// This class contains useful and utility functions that otherwise would have
    /// no other class that can be used throughout the program.
    /// </summary>
    public class Helper
    {

        /// <summary>
        /// A list of valid characters that can be used in the key
        /// </summary>
        private static string VALID_CHARACTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        /// <summary>
        /// A list of ports that have been attempted to be used
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <returns></returns>
        private List<int> attemptedPorts = new List<int>();

        /// <summary>
        /// A hashtable of keys and their hashed values
        /// </summary>
        /// <returns></returns>
        private Hashtable portKeys = new Hashtable();

        /// <summary>
        /// Initialisation of logger class
        /// </summary>
        private Logger logging = new Logger();

        /// <summary>
        /// Initialisation of warn class
        /// </summary>
        private Logger warn = new warn();

        /// <summary>
        /// Initialisation of error class
        /// </summary>
        private Logger error = new error();

        /// <summary>
        /// Initialisation of info class
        /// </summary>
        private Logger info = new info();

        /// <summary>
        /// Initialisation of success class
        /// </summary>
        private Logger success = new success();

        /// <summary>
        /// Initialisation of debug class
        /// </summary>
        private debug debug = new debug();

        private int logLevel = 5;

        /// <summary>
        /// Helper class constructor
        /// </summary>
        public Helper(int logLevel = 5) { this.logLevel = logLevel; }

        /// <summary>
        /// Checks for an open port and then creates a key for that specific port.
        /// </summary>
        /// <returns> A port that is open.</returns>
        public String[] generatePortSpecificHashKey()
        {
            // this will check all the available ports and return a random one available
            Random rnd = new Random();
            int port = rnd.Next(49152, 65535);
            while (this.attemptedPorts.Contains(port))
            {
                port = rnd.Next(49152, 65535);
            }
            try
            {
                // check if the test port is open
                TcpClient temp = new TcpClient();
                temp.ConnectAsync(IPAddress.Any, port);
                temp.Close();
            }
            catch
            {
                // if the port is not open, call back the function
                attemptedPorts.Append(port);
                return generatePortSpecificHashKey();
            }

            

            string unhashed = "";
            for (int i = 0; i < rnd.Next(24, 48); i++)
            {
                unhashed += VALID_CHARACTERS[rnd.Next(0, VALID_CHARACTERS.Length)];
            }

            string key = Convert.ToString(port) + ":" + unhashed;

            // hash the key
            
            double hashedKey = HashGenerator.getStringsHash(key);


            // store the key in a hashtable

            try
            {
                this.portKeys.Add(hashedKey, key);
            }
            catch
            {
                // key already exists, why bother trying to make it complex and adding a new key when we can just completely generate a new key.
                return generatePortSpecificHashKey();
            }


            return new string[] { Convert.ToString(port), unhashed.ToString() };
        }

        /// <summary>
        /// Checks if a given key is valid
        /// </summary>
        /// <param name="key"> The key to check if it is valid</param>
        /// <returns> True if the key is valid, else False.</returns>
        public bool checkKey(string key, int childServerPort)
        {

            string unhashedKey = childServerPort + ":" + key;

            double hashedKey = HashGenerator.getStringsHash(unhashedKey);

            string? keyMappedToPort = this.portKeys[hashedKey] as string;

            if (null == keyMappedToPort || keyMappedToPort != unhashedKey) {
                return false;
            } else {
                this.portKeys.Remove(hashedKey);
                return true;
            }

        }

        /// <summary>
        /// Logs a message to the console with the given type
        /// </summary>
        /// <param name="message">What message you want to log to the cnosole</param>
        /// <param name="type">What type you want to log to the console. Possible </param>
        /// types are: 
        /// <list type="bullet"> <item> <term>warn</term>
        /// <description>Proceeded with a [WARNING] tag and yellow text</description>
        /// </item>
        /// <item>
        /// <term>error</term>
        /// <description>Proceeded with a [ERROR] tag and red text</description>
        /// </item>
        /// <item>
        /// <term>info</term>
        /// <description>Proceeded with a [INFO] tag and blue text</description>
        /// </item>
        /// <item>
        /// <term>success</term>
        /// <description>Proceeded with a [SUCESS] tag and green text</description>
        /// </item>
        /// <item>
        /// <term>debug</term>
        /// <description>Proceeded with a [DEBUG] tag and purple text</description>
        /// </item>
        /// </list>
        public void log(string message, string type)
        {
            switch (type)
            { 
                case "error":
                    // errors are always logged
                    this.error.log(message);
                    break;
                case "warn":
                    if (this.logLevel <= 1) return;
                    this.warn.log(message);
                    break;
                case "success":
                    if (this.logLevel <= 2) return;
                    this.success.log(message);
                    break;
                case "info":
                    if (this.logLevel <= 3) return;
                    this.info.log(message);
                    break;
                case "debug":
                    if (this.logLevel <= 4) return;
                    this.debug.log(message);
                    break;
                default:
                    this.logging.log(message);
                    break;
            }
        }
    }
}
