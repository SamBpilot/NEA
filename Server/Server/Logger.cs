namespace NEA
{

    /// <summary>
    /// Parent Logger class
    /// </summary>
    public class Logger
    {

        /// <summary>
        /// Normal console colour - White
        /// </summary>
        /// <remarks>
        /// To be used after a colour has been set
        /// </remarks>
        private ConsoleColor @default = ConsoleColor.White;

        /// <summary>
        /// Logger class constructor
        /// </summary>
        public Logger() { }

        /// <summary>
        /// Logs a message to the console
        /// </summary>
        /// <param name="message"> The message to log to the console</param>
        public void log(object message) { 
            Console.ForegroundColor = this.getConsoleColor();
            Console.WriteLine(this.getPrefix() + " " + message);
            Console.ForegroundColor = this.getDefault();
        }

        /// <summary>
        /// Get's the defualt colour of the console
        /// </summary>
        /// <returns>The defualt colour of the console</returns>
        public ConsoleColor getDefault() { return this.@default; }

        /// <summary>
        /// Get's the current console colour
        /// </summary>
        /// <returns>The current console colur</returns>
        protected virtual ConsoleColor getConsoleColor() { return Console.ForegroundColor; }

        /// <summary>
        /// Get's the prefix for the log message
        /// </summary>
        /// <returns>The prefix of the log message</returns>
        protected virtual string getPrefix() { return "[LOG]"; }
    }

    /// <summary>
    /// Logger class for warnings
    /// </summary>
    public class warn : Logger
    {

        /// <summary>
        /// Initialises the warn class
        /// </summary>
        public warn() { }

        protected override ConsoleColor getConsoleColor() {
            return ConsoleColor.Yellow;
        }

        protected override string getPrefix() {
            return "[WARNING]";
        }
    }

    /// <summary>
    /// Logger class for information
    /// </summary>
    public class info : Logger
    {
        /// <summary>
        /// Initialises the info class
        /// </summary>
        public info() { }

        protected override ConsoleColor getConsoleColor() {
            return ConsoleColor.Blue;
        }

        protected override string getPrefix() {
            return "[INFO]";
        }

    }

    /// <summary>
    /// Logger class for errors
    /// </summary>
    public class error : Logger
    {

        /// <summary>
        /// Initialises the error class
        /// </summary>
        public error() { }

        protected override ConsoleColor getConsoleColor() {
            return ConsoleColor.Red;
        }
        protected override string getPrefix() {
            return "[ERROR]";
        }
    }

    /// <summary>
    /// Logger class for success
    /// </summary>
    public class success : Logger
    {

        /// <summary>
        /// Initialises the success class
        /// </summary>
        public success() { }

        protected override ConsoleColor getConsoleColor() {
            return ConsoleColor.Green;
        }

        protected override string getPrefix() {
            return "[SUCCESS]";
        }
    }

    /// <summary>
    /// Logger class for debug
    /// </summary>
    public class debug : Logger
    {

        /// <summary>
        /// Initialises the debug class
        /// </summary>
        public debug() { }

        protected override ConsoleColor getConsoleColor() {
            return ConsoleColor.Magenta;
        }

        protected override string getPrefix() {
            return "[DEBUG]";
        }
    }
}
