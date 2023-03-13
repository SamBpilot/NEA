using System.Text.RegularExpressions;

namespace NEA
{
    internal class Program
    {
        static void Main(string[] args)
        {
            #nullable disable
            
            int logLevel = 5; // log everything
            string filePath = ""; // defaults to the current one in xml.cs
            Helper help = new Helper(); // no need to pass log level as the only possible thing that can be logged in this class is errors.


            foreach(String argument in args)
            {
                try{
                    if(argument.Contains("--loglevel"))
                    {
                        // get the log level
                        Regex logLevelRegex = new Regex("--loglevel=[1-5]");
                        if (!logLevelRegex.IsMatch(argument))
                        {
                            help.log("Invalid log level. Please read the documentation for the correct way to use the command line arguments.", "warn");
                            System.Environment.Exit(1);
                        }

                        Match match = logLevelRegex.Match(argument);
                        logLevel = Convert.ToInt32(match.Value.Substring(11));
                        help.log("Log level set to " + logLevel.ToString(), "info");
                    }
                    else if(argument.ToString().Contains("--file"))
                    {
                        // get the file path
                        Regex filePathRegex = new Regex("--file=[a-zA-Z0-9\\\\/\\.:]*");
                        if (!filePathRegex.IsMatch(argument))
                        {
                            help.log("Invalid file path. Please read the documentation for the correct way to use the command line arguments.", "warn");
                            System.Environment.Exit(1);
                        }

                        Match match = filePathRegex.Match(argument);
                        filePath = argument.Substring(7);

                        // check if the file exists
                        if (!File.Exists(filePath))
                        {
                            help.log("I could not find the file you specified. Are you sure it exists?", "warn");
                            System.Environment.Exit(1);
                        }

                        help.log("File path set to " + filePath, "info");
                    }
                } catch (Exception e)
                {
                    help.log(e.Message.ToString(), "error");
                    help.log("Please read the documentation for the correct way to use the command line arguments.", "warn");
                    System.Environment.Exit(1);
                }
            }

            try
            {
                new parentServer(logLevel: logLevel, filePath: filePath).runParentServer();
            }
            catch (Exception e)
            {
                help.log(e.Message.ToString(), "error");
                if (e.StackTrace != null)
                {
                    help.log(e.StackTrace.ToString(), "error");
                }
            }

            // prevents the program from closing. It is here for three reasons.
            // 1. A fail safe, if the program should ever close, it will not close until
            // the user presses enter.
            // 2. Debugging, it is easier to start the server and then run other methods
            // in the main program.
            // 3. Possible future expansion or modification of how the server starts.
            Console.ReadLine();
        }
    }
}
