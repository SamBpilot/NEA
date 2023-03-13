using System.Xml;

namespace NEA
{

    /// <summary>
    /// XML class
    /// </summary>
    public class XMLDataLoader
    {
        
        /// <summary>
        /// The path to the XML file
        /// </summary>
        private string filepath = "";

        /// <summary>
        /// The XML document
        /// </summary>
        private XmlDocument Document = new XmlDocument();

        /// <summary>
        /// Helper class instance
        /// </summary>
        private Helper help = new Helper();

        /// <summary>
        /// Intialisases the XMLDataLoader class.
        /// </summary>
        /// <param name="filePath">The path to the XML file.</param>
        public XMLDataLoader(string filePath) { this.filepath = filePath; }

        /// <summary>
        /// Opens the XML file
        /// </summary>
        /// <returns>True if the xml file was sucesfully opened, else False.</returns>
        public bool open()
        {
            try
            {
                this.Document.Load(this.filepath);
            }
            catch (Exception e)
            {
                this.help.log("Error: " + e.ToString() + " has occured", "error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the nodes in the XML file
        /// </summary>
        /// <param name="stations">The List of stations.</param>
        /// <returns>A new shorterned list of stations that were in the XML file.</returns>
        public List<Station> parse(ref List<Station> stations)
        {
            TraverseNodes(this.Document.ChildNodes, ref stations);

            return stations;
        }

        /// <summary>
        /// Traverses all the nodes in the XML file.
        /// </summary>
        /// <param name="nodes">A list of nodes that the XML file has traversed.</param>
        /// <param name="stations">A reference to the list of stations so that the program can add to them.</param>
        private List<Station> TraverseNodes(XmlNodeList nodes, ref List<Station> stations)
        {
            foreach (XmlNode node in nodes)
            {
                if (node.HasChildNodes)
                {
                    this.TraverseNodes(node.ChildNodes, ref stations);
                }
                else
                {
                    if (node.ParentNode!.Name == "Stop_Name") // we're in a foreach loop, we know that node.ParentNode is not null, especially since we're checking if it has child nodes.
                    {
                        Station s = new Station();
                        s.name = node.InnerText;
                        stations.Add(s);
                    }
                    else if (node.ParentNode.Name == "Daytime_Routes")
                    {
                        foreach (string line in node.InnerText.Split(" "))
                        {
                            stations[stations.Count - 1].addLine(line.ToString());
                        }
                    }
                    else if(node.ParentNode.Name == "GTFS_Latitude")
                    {
                        stations[stations.Count - 1].latitude = decimal.Parse(node.InnerText);
                    }
                    else if(node.ParentNode.Name == "GTFS_Longitude")
                    {
                        stations[stations.Count - 1].longitude = decimal.Parse(node.InnerText);
                    }
                    else if(node.ParentNode.Name == "Station_ID")
                    {
                        stations[stations.Count - 1].id = Convert.ToInt32(node.InnerText);
                    }
                    else if(node.ParentNode.Name == "Surrounding_Stations")
                    {
                        string[] surroundingStations = node.InnerText.Split(" | ");

                        stations[stations.Count-1].addSurroundingStation(surroundingStations);
                    }
                    else if(node.ParentNode.Name == "#document" || node.ParentNode.Name == "row")
                    {
                        // just a simple check to see if this is the root node or a node that we don't care about.
                        // do nothing, simply here to prevent being warned about a node that we should skip anyway.
                    }
                    else
                    {
                        this.help.log("Skipping unkown node " + node.ParentNode.Name, "warn");
                    }
                }
                stations.RemoveAll(x => x.name == null);
            }
            return stations;
        }
    }
}
