using System.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NEA
{

    /// <summary>
    /// MTA Class. Contains all the methods and attributes for directly interacting
    /// with the MTA API.
    /// </summary>
    public class Commands
    {

        /// <summary>
        /// The time a train takes to stop at a station.
        /// </summary>
        public static double TRAIN_STOP_WEIGHT = 1;

        /// <summary>
        /// The time a user takes to walk between two platforms and wait for a train.
        /// </summary>
        public static double TRAIN_CHANGE_WEIGHT = 2;

        /// <summary>
        /// Get's the number of stations on the MTA network.
        /// </summary>
        public int StationCount() {
            return this.stations.Count;
        }
                
        /// <summary>
        /// Creates a list containing all of the stations on the MTA network as well as what line's they are on.
        /// </summary>
        private List<Station> stations = new List<Station>();

        /// <summary>
        /// Creates a list containing all of the NODES on the MTA network, where a node is the intersection of a station and a line.
        /// </summary>
        private List<Node> nodes = new List<Node>();

        /// <summary>
        /// Maps the ID of a station to its respective station object.
        /// </summary>
        private Dictionary<int, Station?> idToName = new Dictionary<int, Station?>();

        /// <summary>
        /// Use the helper class to log messages.
        /// </summary>
        private Helper helperInstance;

        /// <summary>
        /// Initialisation of the MTA class.
        /// </summary>
        public Commands(string? fileName = null, Helper? helperInstance = null)
        {
            this.helperInstance = helperInstance ?? new Helper();
            this.getStations(fileName);
            foreach (Station curr in this.stations) {
                curr.calculateWeights(this.stations);
            }
        }

        /// <summary>
        /// Get's the stations that start with the string
        /// </summary>
        /// <param name="stationNameStart">The station to search for</param>
        /// <returns>A list of all the stations the program found.</returns>
        public List<Station> getStationsStartingWith(string stationNameStart)
        {
            // returns a list of 5 stations that start with the string
            List<Station> possibleStations = this.binarySearch(stationNameStart, true, (string searchString, string name) => name.StartsWith(searchString));

            return possibleStations;
        }

        /// <summary>
        /// Check that the stations are valid
        /// </summary>
        /// <param name="station">The station to check exists.</param>
        /// <returns>True if it found the station, false if it did not.</returns>
        public bool checkStationsAreValid(string station)
        {
            // checks that the stations are valid

            List<Station> validity = this.binarySearch(station, false);

            if (validity.Count != 0 && validity[0].name == station)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get's a station by its id
        /// </summary>
        /// <param name="id">The ID to get the station of</param>
        /// <returns>A nextStation record</returns>
        public Station? getStationById(int id)
        {   
            Station? ret = idToName.GetValueOrDefault(id, null);
            if (null == ret) {
                this.helperInstance.log("Could not find station with id " + id, "warn");
            }
            return ret;
        }

        /// <summary>
        /// Get's a station by its name
        /// </summary>
        /// <param name="name">The name to get the station of</param>
        /// <returns>A nextStation record</returns>
        public Station? getStationByName(string name)
        {
            List<Station> namedStation = this.binarySearch(name, true);
            if (namedStation.Count == 1)
            {
                return namedStation[0];
            }

            this.helperInstance.log("Either I could not find a station with the name " + name + " or there are multiple stations with the same name.", "error");
            this.helperInstance.log("Please use the station's ID instead.","error");

            return null;
        }

        /// <summary>
        /// Routes you from one station to another.
        /// </summary>
        /// <param name="start">The station to start the route from.</param>
        /// <param name="end">The station to route you to.</param>
        /// <returns></returns>
        public string? RouteMe(Station? startStation, Station? endStation)
        {
            if (null == startStation || null == endStation) {
                return null;
            }

            // check that the stations aren't the same
            if (startStation == endStation)
            {
                return "You are already at the station you want to go to.";
            }

            // call the function


            Dictionary<string, Node> map = this.findShortestPath(startStation);


            Route foundRoute = this.walkBack(map, endStation);
            if (foundRoute == null || foundRoute.routeItems.Count == 0)
            {
                this.helperInstance.log("I could not find a route from station with ID " + startStation.id + " to station with ID " + endStation.id, "warn");
                return null;
            }

            List<RouteItem> routeSteps = this.Minify(foundRoute.routeItems);

            string serialiser = JsonSerializer.Serialize(routeSteps);

            this.helperInstance.log("The serialised route is " + serialiser, "debug");

            return serialiser;
        }

        /// <summary>
        /// Reduced the number of steps in a route by combining steps that are on the same train.
        /// </summary>
        /// <param name="routeSteps">The list of routeItems recieved from the findShortestPath method</param>
        /// <returns>A list of route items which are as minified as possible</returns>
        private List<RouteItem> Minify(List<RouteItem> routeSteps)
        {
            for (int i = 0; i < routeSteps.Count; i++)
            {
                RouteItem? previous = null;
                RouteItem current = (RouteItem)routeSteps[i];
                RouteItem? next = null;

                if (i == routeSteps.Count - 1)
                {
                    // break, we can't minify this route any more
                    break;
                }

                next = (RouteItem)routeSteps[i + 1];

                if (i != 0)
                {
                    previous = (RouteItem)routeSteps[i - 1];
                }

                if (current.getOff == next.getOn && current.train == next.train)
                {
                    // we can minify this route
                    // remove the current item
                    routeSteps.RemoveAt(i);
                    routeSteps.RemoveAt(i); // twice to act as i+1 but we've just removed an element at i
                    RouteItem newItem = new RouteItem();
                    newItem.getOff = next.getOff;
                    newItem.getOn = current.getOn;
                    newItem.train = current.train;
                    newItem.stops = current.stops + next.stops;

                    routeSteps.Insert(i, newItem);
                    i--;
                    continue;
                }
            }

            return routeSteps;
        }

        /// <summary>
        /// Finds the weight of the shortest path to all other stations from the start station
        /// </summary>
        /// <param name="startStation">The station we're starting at</param>
        /// <returns>A map of the start station to every other node the algorithm can get to in the graph.</returns>
        private Dictionary<string, Node> findShortestPath(Station startStation)
        {

            Stack routeStack = new Stack();
            Node startNode = startStation.getSubNodes()[0];
            Dictionary<string, double> weights = new Dictionary<string, double>();
            Dictionary<string, Node> parentsMap = new Dictionary<string, Node>();

            weights.Add(startNode.getID(), 0d);
            routeStack.Push(startNode);

            // actual algorithm

            while (routeStack.Count > 0)
            {
                Node? current = routeStack.Pop() as Node;

                if (current == null)
                {
                    continue;
                }

                foreach (Edge e in current.getEdges())
                {
                    Node adjacentNode = e.to;
                    double weight = e.weight;
                    double newWeight = weights[current.getID()] + weight + TRAIN_STOP_WEIGHT;

                    if (!weights.ContainsKey(adjacentNode.getID()))
                    {
                        weights.Add(adjacentNode.getID(), double.PositiveInfinity);
                    }

                    if (weights[adjacentNode.getID()] > newWeight) 
                    {
                        weights[adjacentNode.getID()] = newWeight;
                        parentsMap[adjacentNode.getID()] = current;
                        routeStack.Push(adjacentNode);
                    }
                }
            }

            return parentsMap;
        }

        /// <summary>
        /// Walks back through the parents map to find the route
        /// </summary>
        /// <param name="parentsMap">The map to walk back through</param>
        /// <param name="endStation">The station to stop at.</param>
        /// <returns>A valid route</returns>
        private Route walkBack(Dictionary<string, Node> parentsMap,Station endStation)
        {
            // walk back through the parents map to find the route

            List<Node> nodesRoutedThrough = new List<Node>();
            Node currentNode = endStation.getSubNodes()[0];
            Node? previousNode = null;

            nodesRoutedThrough.Add(currentNode);

            if (parentsMap.ContainsKey(currentNode.getID()))
            {
                previousNode = parentsMap[currentNode.getID()];
            }

            while (previousNode != null)
            {
                nodesRoutedThrough.Add(previousNode);
                currentNode = previousNode;

                if (parentsMap.ContainsKey(currentNode.getID()))
                {
                    previousNode = parentsMap[currentNode.getID()];
                }
                else
                {
                    previousNode = null;
                }
            }

            // turn the list of nodes into a route

            nodesRoutedThrough.Reverse();

            Route route = new Route();
            int iterations = 1;

            while (iterations != nodesRoutedThrough.Count)
            {
                RouteItem item = new RouteItem();
                item.train = nodesRoutedThrough[iterations].Line;
                item.stops = 1;
                item.getOn = nodesRoutedThrough[iterations-1].parent.name;
                item.getOff = nodesRoutedThrough[iterations].parent.name;

                route.routeItems.Add(item);
                iterations++;
            }

            // check to see if the algorithm does something stupid like an internal change
            // before we've even left the first station (one would just go straight to
            // the line anyway) or after we've got to the end station (one would just
            // leave the staion)

            for (int i = 0; i < route.routeItems.Count; i++)
            {
                if (route.routeItems[i].getOn == route.routeItems[i].getOff)
                {
                    route.routeItems.RemoveAt(i);
                    i--;
                }
            }

            return route;
        }

        /// <summary>
        /// Get's all the stations on the MTA network and what line's they are on.
        /// </summary>
        /// <returns>this.stations</returns>
        private List<Station> getStations(string? fileName = null)
        {
            string? path = fileName;
            if (fileName == "")
            {
                path = "Stations.xml";
            }
            
            XMLDataLoader StationData = new XMLDataLoader(path ?? "Stations.xml");

            try
            {
                this.helperInstance.log("Parsing station data", "info");
                this.helperInstance.log(fileName ?? "Defualt data loaded", "info");
                StationData.open();

                this.stations = StationData.parse(ref this.stations);

                foreach (Station curr in this.stations) {
                    this.nodes.AddRange(curr.getSubNodes());
                }

                foreach (Station s in this.stations)
                {
                    this.idToName.Add(s.id, s);
                }

                this.helperInstance.log("Sucesfully parsed stations.xml", "success");
            }
            catch (Exception e)
            {
                this.helperInstance.log("An error occured when attempting to parse station data. Please ensure that the file exists, it is accesible and that it is in the correct format. Please see the documentation for the correct formatting.", "error");
                this.helperInstance.log("Error: " + e.Message, "error");
            }

            this.helperInstance.log("Preparing to sort data", "info");

            Station[] arrayStations = this.stations.ToArray();

            arrayStations = SortMethod(ref arrayStations);

            this.stations = arrayStations.ToList();

            this.helperInstance.log("Sucesfully sorted data", "success");

            return this.stations;
        }
        
        /// <summary>
        /// Method to merge sort the given data (in most cases a direct reference to
        /// this.stations) alphabetically.
        /// </summary>
        /// <param name="data">The array to sort</param>
        /// <returns>A sorted array of data.</returns>
        private Station[] SortMethod(ref Station[] data)
        {
            if (data.Length <= 1)
            {
                return data;
            }

            int midpoint = data.Length / 2;

            Station[] Left = new Station[midpoint];
            Station[] Right;
            if (data.Length % 2 == 0)
            {
                Right = new Station[midpoint];
            }
            else
            {
                Right = new Station[midpoint + 1];
            }

            for (int j = 0; j < midpoint; j++)
            {
                Left[j] = data[j];
            }

            for (int j = 0; j < Right.Length; j++)
            {
                Right[j] = data[midpoint + j];
            }

            Left = SortMethod(ref Left);
            Right = SortMethod(ref Right);
            Station[] result = merge(Left, Right);

            return result;
        }

        /// <summary>
        /// Merges two 2d arrays together.
        /// </summary>
        /// <param name="left">The left half of the array to merge</param>
        /// <param name="right">The right half of the array to merge</param>
        /// <returns>One 2d array merged and sorted.</returns>
        private Station[] merge(Station[] left, Station[] right)
        {

            int resultLength = right.Length + left.Length;
            Station[] result = new Station[resultLength];
            
            int indexLeft = 0, indexRight = 0, indexResult = 0;
            // while either array still has an element
            while (indexLeft < left.Length || indexRight < right.Length)
            {
                // if both arrays have elements
                if (indexLeft < left.Length && indexRight < right.Length)
                {
                    // If item on left array is less than item on right array, add that item
                    // to the result array
                    if (compare(left[indexLeft].name, right[indexRight].name) == true)
                    {
                        result[indexResult] = left[indexLeft];
                        indexLeft++;
                        indexResult++;
                    }
                    // else the item in the right array wll be added to the results array
                    else
                    {
                        result[indexResult] = right[indexRight];
                        indexRight++;
                        indexResult++;
                    }
                }
                // if only the left array still has elements, add all its items to the
                // results array
                else if (indexLeft < left.Length)
                {
                    result[indexResult] = left[indexLeft];
                    indexLeft++;
                    indexResult++;
                }
                // if only the right array still has elements, add all its items to the
                // results array
                else if (indexRight < right.Length)
                {
                    result[indexResult] = right[indexRight];
                    indexRight++;
                    indexResult++;
                }
            }
            return result;
        }

        /// <summary>
        /// Compares two strings and returns true if the first string is
        /// alphabetically before the second.
        /// </summary>
        /// <param name="a">The first string to compare</param>
        /// <param name="b">The second string to compare</param>
        /// <returns>True if a is before b in the alphabet, else False.</returns>
        private bool compare(String a, String b)
        {
            int lengthMovedAlong = 0;

            string no_spaces_a = a.Replace(" ", "");
            string no_spaces_b = b.Replace(" ", "");

            if (no_spaces_a == null || no_spaces_b == null)
            {
                return false;
            }

            string b_lower = no_spaces_b.ToLower();

            Regex exclusivelyNumbers = new Regex(@"^\d{1,3}");
            Match aMatch = exclusivelyNumbers.Match(no_spaces_a);
            Match bMatch = exclusivelyNumbers.Match(no_spaces_b);

            if (aMatch.Success && bMatch.Success)
            {
                int aInt = Convert.ToInt16(aMatch.Value);
                int bInt = Convert.ToInt16(bMatch.Value);

                if (aInt < bInt)
                {
                    return true;
                }
                else if (aInt > bInt)
                {
                    return false;
                }
                else
                {
                    // so it's the same number, but we need to check the rest of the string so we can pass.
                }
            }

            foreach (char c in no_spaces_a.ToLower())
            {
                if (c == ' ' && lengthMovedAlong >= b_lower.Length == false)
                {
                    // b must be longer than a and so is greater than
                    // help.log(a + " is to the right of " + b, "debug");
                    return false;
                }
                else if (lengthMovedAlong >= b_lower.Length && c != ' ')
                {
                    // a must be longer than b and so is greater than
                    // help.log(a + " is to the left of " + b, "debug");
                    return true;
                }


                if (Convert.ToInt16(c) < Convert.ToInt16(b_lower[lengthMovedAlong]))
                {
                    // to the left of
                    // help.log(a + " is to the left of " + b, "debug");
                    return true;
                }
                else if (Convert.ToInt16(c) > Convert.ToInt16(b_lower[lengthMovedAlong]))
                {
                    // to the right of
                    // help.log(a + " is to the right of " + b, "debug");
                    return false;
                }
                else
                {
                    lengthMovedAlong++;
                }
            }
            // it'll never get here
            return false;
        }

        /// <summary>
        /// Binary searches the stations array for a station that starts with the
        /// string
        /// </summary>
        /// <param name="station">The station to look for.</param>
        /// <param name="getNextFive">Optional, get's five stations total that also
        /// begin with the station name.</param> <returns>A 2d array of all the
        /// stations it found.</returns>
        /// <param name="compareFunction">The function to use to compare the two strings.</param>
        private List<Station> binarySearch(string station = "", bool getNextFive = false, Func<string, string, bool>? compareFunction = null)
        {

            if (null == compareFunction) {
                compareFunction = (string searchString, string name) => name == searchString;
            }
            
            if (station == "")
            {
                return new List<Station>();
            }

            List<Station> potentialStations = new List<Station>();

            // perform a binary search

            int max = this.stations.Count;
            int mid = max / 2;
            int lower_bound = 0;
            int upper_bound = max;

            int maximum_iterations = 0;

            while (mid != max && maximum_iterations <= 500)
            {
                if (compareFunction(station, this.stations[mid].name))
                {
                    // we have found a station that startswith the string, therefore we can
                    // append it to the potentialStations array
                    potentialStations.Add(this.stations.ElementAt(mid));

                    if (getNextFive == false)
                    {
                        break;
                    }

                    // now we need to find the next 4 stations that start with the string.
                    // To do this we search the next 4 stations and the previous 4 stations
                    int i = 1;
                    int found = 1;
                    while (i != 5)
                    {

                        if (mid + i < this.stations.Count && compareFunction(station, this.stations[mid + i].name) && found != 5)
                        {
                            potentialStations.Add(this.stations.ElementAt(mid + i));
                            found++;
                        }
                        if (mid - i > 0 && compareFunction(station, this.stations[mid - i].name) && found != 5)
                        {
                            potentialStations.Add(this.stations.ElementAt(mid - i));
                            found++;
                        }
                        i++;
                    }
                    break;
                }
                else if (compare(this.stations.ElementAt(mid).name, station))
                {
                    // C# has determined that the sation must be to the right (the upper
                    // half) of the array so we can set the lower bound to the mid point and
                    // recalculate the mid point

                    lower_bound = mid;
                    mid = (upper_bound + lower_bound) / 2;
                }
                else if (compare(this.stations.ElementAt(mid).name, station) == false)
                {
                    // C# has determined that the station must be to the left (the lower
                    // half) of the array so we can set the upper bound to the mid point and
                    // recalculate the mid point

                    upper_bound = mid;
                    mid = (upper_bound + lower_bound) / 2;
                }
                else
                {
                    // something has gone wrong
                    break;
                }
                maximum_iterations++;
            }
            return potentialStations;
        }

    }
}
