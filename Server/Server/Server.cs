using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;


namespace NEA
{

    /// <summary>
    /// Class containing methods and attributes required to run the server
    /// </summary>
    public class Server
    {

        /// <summary>
        /// The path to the stations file, defualts to the Stations.xml file that is bundled with the server.
        /// </summary>
        protected string filePath = "";

        /// <summary>
        /// The log level of the server. See the documentation for log levels.
        /// </summary>
        protected int logLevel = 5;

        /// <summary>
        /// Whether the server is retrying to start.
        /// </summary>
        protected bool isRetrying = true;

        /// <summary>
        /// The IP address of the server
        /// </summary>
        protected IPAddress? address;

        /// <summary>
        /// The port that the server is running on. Default is 55600 but will be
        /// overridden if it is a child server.
        /// </summary>
        protected int port = 55600;

        /// <summary>
        /// TcpListener object that is used to listen for incoming connections
        /// </summary>
        protected TcpListener? TCP;

        /// <summary>
        /// Instance of the Helper class
        /// </summary>
        protected static Helper help;

        /// <summary>
        /// Intialises the server
        /// </summary>
        /// <param name="port">The port to use, defualts to 55600 as this is only used
        /// for the child server.</param>
        public Server(int port = 55600, int logLevel = 5, string filePath = "")
        {
            // validate that the port is within range
            if (port < 49152 || port > 65535) // Ephemeral port range according too IANA (Internet Assigned Numbers Authority)
            {
                throw new ArgumentOutOfRangeException("port", "Port must be between 0 and 65535"); // It will never get down here unless the user is explicitly trying to break the program.
            }
            this.port = port;
            this.logLevel = logLevel;
            this.filePath = filePath;

            if (Server.help == null)
            {
                Server.help = new Helper(this.logLevel);
            }
        }

        /// <summary>
        /// Starts the server
        /// </summary>
        /// <param name="port">Which port to start the server on. Defualts to 55600 as
        /// this is only used for the child server.</param>
        protected void startServer(int port = 55600)
        {
            this.isRetrying = true;
            while (this.isRetrying == true)
            {
                try
                {
                    this.TCP = new TcpListener(IPAddress.Any, port);
                    if (this.address == null)
                    {
                        this.address = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0];
                    }
                    this.TCP.Start();
                    Server.help.log("Server started on " + this.address + ":" + this.port, "success");
                    Server.help.log("Listening on " + this.address + ":" + this.port, "info");
                    this.isRetrying = false;
                }
                catch (Exception e)
                {
                    Server.help.log("Error, failed to start the server : " + e.Message, "error");
                    Server.help.log("Retrying in 5 seconds", "error");
                    Thread.Sleep(5000);
                }
            }
        }

        /// <summary>
        /// Adds padding to the data sent to the client and also adds any parameters
        /// </summary>
        /// <param name="parameters">Any parameters to be sent to the client</param>
        /// <returns>A string capable of being sent to the client</returns>
        protected string encode(string[] parameters)
        {
            string response = "<";
            if (parameters.Length > 0)
            {
                foreach (string parameter in parameters)
                {
                    response += parameter + "{}";
                }

                response = response.Substring(0, response.Length - 2);
            }

            response += ">";
            return response;
        }

        /// <summary>
        /// Removes the padding from the data received from the client and returns any parameters sent with the request.
        /// </summary>
        /// <param name="data">The data received from the client</param>
        /// <returns>The parameters in a message from the client</returns>
        protected string[]? decode(string data)
        {
            string relevant_data;
            try
            {
                relevant_data = data.Split(":")[1];
            }
            catch (Exception)
            {
                return null;
            }
            relevant_data = relevant_data.Replace(">", "");

            String[] new_data = relevant_data.Split("{}");

            new_data = new_data.Skip(1).ToArray();

            new_data[0] = new_data[0].Replace("\r\n", "");

            return new_data;
        }
    }

    /// <summary>
    /// The child server class
    /// </summary>
    public class childServer : Server
    {
        
        /// <summary>
        /// Whether this is the first response from the server
        /// </summary>
        private bool firstResponse = true;

        /// <summary>
        /// Commands that can be ran by the child server
        /// </summary>
        private Commands mta;

        /// <summary>
        /// Initialiser of the child server class.
        /// </summary>
        /// <param name="port">The port to start the child server on</param>
        public childServer(int port, int logLevel = 5, string filePath = "") : base(port: port, logLevel: logLevel, filePath: filePath)
        {
            this.mta = new Commands(this.filePath, Server.help);
            this.startServer(port);
            this.runChildServer(port);

            if (Server.help == null)
            {
                // it will never get down here unless the user is explicitly trying to break the program.
                throw new Exception("Helper is null, are you trying to start the child server before calling the startServer method?");
            }
        }

        /// <summary>
        /// The main loop of the child server
        /// </summary>
        /// <param name="port">The port to run the child server on.</param>
        public void runChildServer(int port)
        {
            if (this.TCP == null)
            {
                throw new Exception("TCP is null, are you trying to start the child server before calling the startServer method?");
                // it will never get down here unless the user is explicitly trying to break the program.
            }

            string reply = string.Empty;
            Socket handler = this.TCP.AcceptSocket();
            Server.help.log("Server accepted connection on address " + handler.LocalEndPoint, "info");

            while (true)
            {
                byte[] bytes = new byte[1024];
                int bytes_rec = handler.Receive(bytes);
                string data = Encoding.ASCII.GetString(bytes, 0, bytes_rec);

                if (bytes_rec == 0)
                {
                    // attempt at fixing connection reset by peer
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Disconnect(true);
                    handler.Close();
                    return;
                }

                Regex[] validCommands = new Regex[4];

                validCommands[0] = new Regex("<Key:{}[a-zA-Z0-9: ]*>");// Key
                validCommands[1] = new Regex("<SBW:{}[a-zA-Z0-9 ]*>");// Stations Beginning With
                validCommands[2] = new Regex("<IVS:{}[a-zA-Z0-9 ]*>");//Is Valid Station
                validCommands[3] = new Regex("<GR:{}[a-zA-Z0-9 ]*{}[a-zA-Z0-9 ]*>");// get route

                if (data.Contains("<EOF>")) // end of file
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Disconnect(true);
                    handler.Close();

                    return; // if we return this thread closes itself.

                }
                else if (this.firstResponse)
                {
                    if (validCommands[0].IsMatch(data))
                    {
                        // a key was sent so validate it
                        string? parameters = this.decode(data)?[0];

                        if (parameters == null)
                        {
                            Server.help.log($"Key for port {port} was invalid", "warn");
                            reply = "No key was sent on first interaction with chlid server. Please rety. or send eof to end connection";
                            handler.Send(Encoding.ASCII.GetBytes(reply));
                        }
                        else if(Server.help.checkKey(parameters, port))
                        {
                            this.firstResponse = false;
                            reply = "<200>";
                            handler.Send(Encoding.ASCII.GetBytes(reply));
                            Server.help.log($"Key for port {port} was valid", "success");
                        }
                        else
                        {
                            Server.help.log($"Key for port {port} was invalid", "warn");
                            reply = "No key was sent on first interaction with chlid server. Please rety. or send eof to end connection";
                            handler.Send(Encoding.ASCII.GetBytes(reply));
                        }
                    }
                    else
                    {
                        Server.help.log($"No key was sent on first interaction with chlid server on port {port}", "warn");
                        reply = "No key was sent on first interaction with chlid server. Please rety. or send eof to end connection";
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                    }
                }
                else if (validCommands[1].IsMatch(data)) // Stations Begining With
                {
                    string? parameters = this.decode(data)?[0];
                    if (parameters == null)
                    {
                        Server.help.log($"Server recieved stations begining with command with no parameters", "warn");
                        reply = "No parameters were sent with the stations begining with command. Please rety. or send eof to end connection";
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                        continue;
                    }

                    Server.help.log($"Server recieved stations begining with command with stations {parameters}", "info");

                    List<Station> stations = this.mta.getStationsStartingWith(parameters);

                    List<string> stationNames = new List<string>();

                    foreach (Station station in stations)
                    {
                        stationNames.Add(station.ToString());
                        Server.help.log(station.ToString(), "debug");
                    }

                    reply = this.encode(stationNames.ToArray());

                    Server.help.log($"Server sending stations begining with command with stations {reply}", "info");

                    handler.Send(Encoding.ASCII.GetBytes(reply));
                }
                else if (validCommands[2].IsMatch(data)) // Valid Station
                {
                    string? parameters = this.decode(data)?[0];
                    if (parameters == null)
                    {
                        Server.help.log($"Server recieved Is Valid Station command with no parameters", "warn");
                        reply = "No parameters were sent with the Is Valid Station command. Please rety. or send eof to end connection";
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                        continue;
                    }

                    Server.help.log($"Server recieved Is Valid Station command with station {parameters}", "info");

                    bool isValid = this.mta.checkStationsAreValid(parameters);

                    reply = this.encode(new string[] { isValid.ToString() });

                    handler.Send(Encoding.ASCII.GetBytes(reply));
                }
                else if (validCommands[3].IsMatch(data)) // Get Route
                {

                    String[]? parameters = this.decode(data);
                    if (parameters == null)
                    {
                        Server.help.log($"Server recieved Get Route command with no parameters", "warn");
                        reply = "No parameters were sent with the Get Route command. Please rety. or send eof to end connection";
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                        continue;
                    }

                    int parameter1 = Convert.ToInt32(parameters[0]);
                    int parameter2 = Convert.ToInt32(parameters[1]);
                    
                    
                    Server.help.log($"Server recieved Get Route command with parameters {parameter1} {parameter2} on port {port}", "info");

                    // actually route them.

                    Station? s1 = this.mta.getStationById(parameter1);
                    Station? s2 = this.mta.getStationById(parameter2);

                    if (null == s1 || null == s2) {
                        // Error response.
                        reply = this.encode(new string[] { "Invalid Station" });
                    } else {
                        string? route = this.mta.RouteMe(s1, s2);
                        if (route == null)
                        {
                             reply = this.encode(new string[] { "No route found" });
                        }
                        else
                        {
                            reply = this.encode(new string[] { route });
                        }
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                    }
                }
                else
                {
                    reply = $"Server recieved data {data} but no command was found or the request was malformed";
                    try
                    {
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                        Server.help.log("Server recieved data " + data + " but no command was found or the request was malformed", "warn");
                    }
                    catch
                    {
                        Server.help.log("Server failed to contact client, most likely due to client closing connection", "warn");

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Disconnect(true);
                        handler.Close();

                        return; // if we return this thread closes itself.
                    }
                }
            }
        }

    }

    /// <summary>
    /// The parent server class
    /// </summary>
    public class parentServer : Server
    {

        /// <summary>
        /// Initialiser
        /// </summary>
        public parentServer(int logLevel = 5, string filePath = "") : base(logLevel: logLevel, filePath: filePath)
        {
            this.startServer();

            if (Server.help == null)
            {
                Server.help = new Helper(this.logLevel);
            }
        }

        /// <summary>
        /// Runs the parents server
        /// </summary>
        public void runParentServer()
        {
            if (this.TCP == null)
            {
                throw new Exception("TCP is null, are you trying to start the child server before calling the startServer method?");
                // it will never get down here unless the user is explicitly trying to break the program.
            }
            
            Socket handler = this.TCP.AcceptSocket();
            handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Server.help.log("Server accepted connection on address " + handler.LocalEndPoint, "info");
            while (true)
            {
                byte[] bytes = new byte[1024];
                int bytes_rec = handler.Receive(bytes);
                string data = Encoding.ASCII.GetString(bytes, 0, bytes_rec);

                Regex rg = new Regex("<bind>"); // binding

                if (data.Contains("<EOF>")) // end of file
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Disconnect(true);
                    handler.Close(); 

                    handler = this.TCP.AcceptSocket();
                    handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                else if (rg.IsMatch(data)) // Bind
                {
                    Server.help.log("Parent server recieved bind command", "info");

                    string[] portAndHashKey = Server.help.generatePortSpecificHashKey();
                    string port = portAndHashKey[0];
                    string portHashKey = portAndHashKey[1];

                    Server.help.log("Server generated key " + portHashKey + " for port " + port, "info");

                    handler.Send(Encoding.ASCII.GetBytes("<Port:{}" + port + "{}Key:{}" + portHashKey + ">"));

                    // close the connection to this specific client on this port
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Disconnect(true);
                    handler.Close();

                    Thread childThread = new Thread(() => new childServer(port: Convert.ToInt32(port), logLevel: this.logLevel, filePath: this.filePath));
                    childThread.Start();

                    // reopen the port
                    handler = this.TCP.AcceptSocket();
                    handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    
                }
                else
                {
                    string reply = $"Server recieved data {data} but no command was found or the request was malformed";
                    try
                    {
                        handler.Send(Encoding.ASCII.GetBytes(reply));
                        Server.help.log("Server recieved data " + data + " but no command was found or the request was malformed on parent server", "warn");
                    }
                    catch
                    {
                        Server.help.log("Server failed to contact client, most likely due to client closing connection", "warn");

                        handler.Shutdown(SocketShutdown.Both);
                        handler.Disconnect(true);
                        handler.Close();

                        handler = this.TCP.AcceptSocket();
                        handler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    }
                }
            }
        }
    }
}
