using System.Text.RegularExpressions;

namespace NEA
{

    /// <summary>
    /// A route class
    /// </summary>
    public class Route {

        /// <summary>
        /// Each route item in the route
        /// </summary>
        public List<RouteItem> routeItems = new List<RouteItem>();
    }

    /// <summary>
    /// Route Item record
    /// </summary>
    [Serializable]
    public record RouteItem
    {
        /// <summary>
        /// Where the user should get on
        /// </summary>
        public string? getOn { get; set; }

        /// <summary>
        /// Where the user should get off
        /// </summary>
        public string? getOff { get; set; }

        /// <summary>
        /// The number of stops the user should travel for.
        /// </summary>
        public int? stops { get; set; }

        /// <summary>
        /// The train the user should travel on
        /// </summary>
        public string? train { get; set; }
    }

    /// <summary>
    /// Station class
    /// </summary>
    [Serializable]
    public class Station
    {
        public override string ToString()
        {
            return "[Id: " + this.id + ", Name: " + this.name + ", Lines: " + string.Join( ",", this.lines.ToArray() ) + "]";
        }


        /// <summary>
        /// The name of the station
        /// </summary>
        public string name {get; set;}

        /// <summary>
        /// The latitude of the station
        /// </summary>
        public decimal latitude {get; set;}

        /// <summary>
        /// The longitude of the station
        /// </summary>
        public decimal longitude {get; set;}

        /// <summary>
        /// The ID of the station
        /// </summary>
        public int id {get; set;}

        /// <summary>
        /// A list of nodes that the station contains
        /// </summary>
        private List<Node> subNodes {get; set;}

        /// <summary>
        /// A list of all the surrounding stations ID's and (if applicable) what line they are on.
        /// </summary>
        private List<string> surroundingStations = new List<string>();

        private List<string> lines = new List<string>();
        
        /// <summary>
        /// The station constructor
        /// </summary>
        public Station()
        {
            this.subNodes = new List<Node>();
            this.name = string.Empty; // keeps the compiler happy
            // happy compiler happy life
        }

        /// <summary>
        /// Add a line to this station
        /// </summary>
        /// <param name="lineName">What line to add</param>
        public void addLine(String lineName)
        {
            this.lines.Add(lineName);
            this.subNodes.Add(new Node(this, lineName));
        }

        /// <summary>
        /// Get all the stations connections
        /// </summary>
        /// <returns>A list of nextStation records</returns>
        public List<Node> getSubNodes()
        {
            // return the connections
            return this.subNodes;
        }

        /// <summary>
        /// Add's a surrounding station to the station
        /// </summary>
        /// <param name="surroundingStationIDs"></param>
        public void addSurroundingStation(string[] surroundingStationIDs)
        {
            if (this.surroundingStations == null)
            {
                this.surroundingStations = new List<string>();
            }
            foreach (string s in surroundingStationIDs)
            {
                this.surroundingStations.Add(s);
            }
        }

        /// <summary>
        /// Get's the node on a specific line
        /// </summary>
        /// <param name="line">What line to get the node of</param>
        /// <returns></returns>
        public Node? getNodeOnLine(string line) {
            Node? ret = this.subNodes.Find((x) => x.Line == line);
            return null != ret ? ret : null;
        }

        /// <summary>
        /// Calculates the weights of the surrounding stations
        /// </summary>
        /// <param name="masterStationList">The list of all stations.</param>
        public void calculateWeights(List<Station> masterStationList)
        {
            foreach (string nextStationID in this.surroundingStations)
            {
                // get the station's ID
                Regex r = new Regex(@"^\d{1,3}");
                int id = Int32.Parse(r.Match(nextStationID).Value);
                List<string> onlyAddToTheseLines = new List<string>();

                Station? nextStation = masterStationList.Find((x) => x.id == id);
                if (null == nextStation) {
                    continue;
                }


                // get the lines which should go here

                if (nextStationID.Contains("]"))
                {
                    // get the lines which should go here
                    Regex r2 = new Regex(@"\[(.*?)\]");
                    string lines = r2.Match(nextStationID).Value;
                    lines = lines.Replace("[", "");
                    lines = lines.Replace("]", "");
                    onlyAddToTheseLines = lines.Split(",").ToList();
                }

                decimal changeInLat = this.latitude - nextStation.latitude;
                decimal changeInLong = this.longitude - nextStation.longitude;

                // use the changes in latitude and longitude to calculate the hypotenuse

                float hypotenuse = (float)Math.Sqrt(Math.Pow(Convert.ToDouble(changeInLat), 2) + Math.Pow(Convert.ToDouble(changeInLong), 2));

                // use the hypotenuse to calculate the time to travel between the two stations by dividing by the average speed of a NYC subway train (17.4 mph) and multiply by 10000 to get a rough "guestimate" of the time between each station

                double time = (hypotenuse / 17.4f) * 10000;

                // now round becuase there's no point in having a *hideuosly* accurate time

                time = Math.Round(time, 2);

                foreach (Node n in this.subNodes)
                {
                    if (onlyAddToTheseLines.Count > 0 && !onlyAddToTheseLines.Contains(n.Line)) {
                        continue;
                    }
                    Node? nextNodeOnSameLine = nextStation.getNodeOnLine(n.Line);
                    if (null == nextNodeOnSameLine) {
                        continue;
                    }
                    
                    n.addEdge(nextNodeOnSameLine, time);
                }
            }

            // Link all the nodes which represent this station to each other, with a weight of 2.
            // This means the graph shows a cost of changing lines but there will be no cost
            // if the in and out station are both on the SAME line.  Only CHANGING lines
            // incurs the cost of 2 of going from Nx to Ny (where x and y are different lines at the same station N).
            foreach (Node n in this.subNodes)
            {
                foreach (Node curr in this.subNodes.FindAll((x) => x != n))
                {
                    n.addEdge(curr, Commands.TRAIN_CHANGE_WEIGHT);
                    
                }
            }
        }
    }

    /// <summary>
    /// A record to store information about the edge
    /// </summary>
    /// <value></value>
    [Serializable]
    public record Edge
    {

        /// <summary>
        /// The edge constructor
        /// </summary>
        /// <param name="to">Where this edge goes</param>
        /// <param name="weight">How long it takes to travel this edge</param>
        public Edge(Node to, double weight)
        {
            this.to = to;
            this.weight = weight;
        }

        /// <summary>
        /// Where this edge goes
        /// </summary>
        public Node to {get; set;}

        /// <summary>
        /// How long it takes to travel this edge
        /// </summary>
        public double weight {get; set;}
    }

    /// <summary>
    /// A record to store information about the next station
    /// </summary>
    /// <value>stationID, timeTo</value>
    [Serializable]
    public record Node
    {
        /// <summary>
        /// Line
        /// </summary>
        /// <value>The line of the next station</value>
        public string Line { get; set; }

        /// <summary>
        /// Parent
        /// </summary>
        /// <value>The parent station this node belongs to.</value>
        public Station parent {get;} 

        /// <summary>
        /// The ID of this node
        /// </summary>
        public string getID()
        {
            return this.parent.id + ":" + this.Line;
        }

        /// <summary>
        /// The edges of this node
        /// </summary>
        /// <typeparam name="Edge"></typeparam>
        private List<Edge> edges = new List<Edge>();

        /// <summary>
        /// nextStation constructor
        /// </summary>
        public Node(Station parent, string line)
        {
            this.parent = parent;
            this.Line = line;
        }

        /// <summary>
        /// Adds an edge to the node
        /// </summary>
        /// <param name="n">The node it goes to</param>
        /// <param name="time">The time it takes to get there</param>
        public void addEdge(Node n, double time)
        {
            // creates an edge record and adds it to a list of edges
            this.edges.Add(new Edge(n, time));
        }

        /// <summary>
        /// Gets the edges of this node
        /// </summary>
        public List<Edge> getEdges()
        {
            return this.edges;
        }
    }
}
