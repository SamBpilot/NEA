// NOTE: This file is NOT to be distributed with the server. It is only for testing purposes.

//NOTE: Nodes that are internal to a station will always be the last in node.getEdges() and node.getSubNodes()
// This is because they are added last.

namespace Server.Tests
{
    [Trait("Graph", "Graph Tests")]
    public class GraphTest
    {
        [Fact]
        public void GRAPH_ONE_STATION_TEST()
        {
            Commands graph = new Commands(fileName: "../../../testStations/oneStation.xml");

            Assert.Equal(graph.getStationById(522), graph.getStationByName("Test St")); // tests that the file loaded in correctly.

            Station? station1 = graph.getStationById(522);
            if (null == station1) {
                return;
            }

            Assert.True(station1.getSubNodes().Count == 1); // tests that the station has one line

            Node n = station1.getSubNodes()[0];

            Assert.Equal("1", n.Line);
            Assert.Equal(station1, n.parent);
            Assert.Empty(n.getEdges());

        }

        [Fact]
        public void GRAPH_ONE_STATION_MULTIPLE_LINES_TEST()
        {
            /* ------------------------------- // Loading ------------------------------- */
            Commands graph = new Commands(fileName: "../../../testStations/oneStationMultipleLines.xml");

            Assert.Equal(graph.getStationById(522), graph.getStationByName("Test St")); // tests that the file loaded in correctly.

            Station? station1 = graph.getStationById(522);
            Assert.NotNull(station1);
            
            if (null == station1) {
                return;
            }

            /* ----------------------- // Testing sub nodes exist ----------------------- */

            Assert.True(station1.getSubNodes().Count == 3); // tests that the station has 3 nodes
            Node n1 = station1.getSubNodes()[0];
            Node n2 = station1.getSubNodes()[1];
            Node n3 = station1.getSubNodes()[2];
            
            /* -------------------------------- // node 1 ------------------------------- */

            Assert.Equal("1", n1.Line);
            Assert.Equal(station1, n1.parent);
            Assert.Equal(2, n1.getEdges().Count);
         

            /* -------------------------------- // node 2 ------------------------------- */
            
            Assert.Equal(new Edge(n2, 2), n1.getEdges()[0]);
            Assert.Equal("2", n2.Line);
            Assert.Equal(station1, n2.parent);
            Assert.Equal(2, n2.getEdges().Count);
            Assert.Equal(new Edge(n1, 2), n2.getEdges()[0]);

            /* -------------------------------- // node 3 ------------------------------- */

            Assert.Equal(new Edge(n3, 2), n1.getEdges()[1]);
            Assert.Equal("3", n3.Line);
            Assert.Equal(station1, n3.parent);
            Assert.Equal(2, n3.getEdges().Count);
            Assert.Equal(new Edge(n1, 2), n3.getEdges()[0]);
            Assert.Equal(new Edge(n2, 2), n3.getEdges()[1]);
            Assert.Equal(new Edge(n3, 2), n2.getEdges()[1]);
        }

        [Fact]
        public void GRAPH_TWO_STATIONS_SAME_LINE_TEST()
        {
            /* ------------------------------- // Loading ------------------------------- */

            Commands graph = new Commands(fileName : "../../../testStations/twoStationsSameLine.xml");

            Assert.Equal(graph.getStationById(522), graph.getStationByName("Test St")); // tests that the file loaded in correctly.
            Assert.Equal(graph.getStationById(523), graph.getStationByName("Fake Road")); // tests that the file loaded in correctly.

            Station? station1 = graph.getStationById(522);
            Assert.NotNull(station1);
            
            if (null == station1) {
                return;
            }

            Station? station2 = graph.getStationById(523);
            Assert.NotNull(station2);
            
            if (null == station2) {
                return;
            }

            /* ----------------------- // Testing sub nodes exist ----------------------- */

            Assert.Equal(1, station1.getSubNodes().Count); // tests that the station has 1 node
            Assert.Equal(1, station2.getSubNodes().Count); // tests that the station has 1 node

            Node n1 = station1.getSubNodes()[0];
            Node n2 = station2.getSubNodes()[0];

            /* -------------------------------- // node 1 ------------------------------- */

            Assert.Equal("1", n1.Line);
            Assert.Equal(station1, n1.parent);
            Assert.Equal(1, n1.getEdges().Count);
            Assert.Equal(new Edge(n2, 2.94), n1.getEdges()[0]); 

            /* -------------------------------- // node 2 ------------------------------- */

            Assert.Equal("1", n2.Line);
            Assert.Equal(station2, n2.parent);
            Assert.Equal(1, n2.getEdges().Count);
            Assert.Equal(new Edge(n1, 2.94), n2.getEdges()[0]); 

        }

        [Fact]
        public void GRAPH_TWO_STATIONS_MULTIPLE_LINES_TEST()
        {
            /* ------------------------------- // Loading ------------------------------- */

            Commands graph = new Commands(fileName : "../../../testStations/twoStationsMultipleLines.xml");

            Assert.Equal(graph.getStationById(522), graph.getStationByName("Test St")); // tests that the file loaded in correctly.
            Assert.Equal(graph.getStationById(523), graph.getStationByName("Fake Road")); // tests that the file loaded in correctly.

            Station? station1 = graph.getStationById(522);
            Assert.NotNull(station1);
            if (null == station1) {
                return;
            }
            
            Station? station2 = graph.getStationById(523);
            Assert.NotNull(station2);
            if (null == station2) {
                return;
            }

            /* ----------------------- // Testing sub nodes exist ----------------------- */

            Assert.Equal(3, station1.getSubNodes().Count); // tests that the station has 3 nodes
            Assert.Equal(3, station2.getSubNodes().Count); // tests that the station has 3 nodes

            Node n1 = station1.getSubNodes()[0];
            Node n2 = station1.getSubNodes()[1];
            Node n3 = station1.getSubNodes()[2];

            Node n4 = station2.getSubNodes()[0];
            Node n5 = station2.getSubNodes()[1];
            Node n6 = station2.getSubNodes()[2];

            /* -------------------------------- // Node 1 ------------------------------- */

            Assert.Equal("1", n1.Line);
            Assert.Equal(station1, n1.parent);
            Assert.Equal(3, n1.getEdges().Count);
            Assert.Equal(new Edge(n4, 2.94), n1.getEdges()[0]); 
            Assert.Equal(new Edge(n2, 2), n1.getEdges()[1]);
            Assert.Equal(new Edge(n3, 2), n1.getEdges()[2]);

            /* -------------------------------- // Node 2 ------------------------------- */

            Assert.Equal("2", n2.Line);
            Assert.Equal(station1, n2.parent);
            Assert.Equal(3, n2.getEdges().Count);
            Assert.Equal(new Edge(n5, 2.94), n2.getEdges()[0]); 
            Assert.Equal(new Edge(n1, 2), n2.getEdges()[1]);
            Assert.Equal(new Edge(n3, 2), n2.getEdges()[2]);

            /* -------------------------------- // Node 3 ------------------------------- */

            Assert.Equal("3", n3.Line);
            Assert.Equal(station1, n3.parent);
            Assert.Equal(3, n3.getEdges().Count);
            Assert.Equal(new Edge(n6, 2.94), n3.getEdges()[0]); 
            Assert.Equal(new Edge(n1, 2), n3.getEdges()[1]);
            Assert.Equal(new Edge(n2, 2), n3.getEdges()[2]);

            /* -------------------------------- // Node 4 ------------------------------- */

            Assert.Equal("1", n4.Line);
            Assert.Equal(station2, n4.parent);
            Assert.Equal(3, n4.getEdges().Count);
            Assert.Equal(new Edge(n1, 2.94), n4.getEdges()[0]); 
            Assert.Equal(new Edge(n5, 2), n4.getEdges()[1]);
            Assert.Equal(new Edge(n6, 2), n4.getEdges()[2]);

            /* -------------------------------- // Node 5 ------------------------------- */

            Assert.Equal("2", n5.Line);
            Assert.Equal(station2, n5.parent);
            Assert.Equal(3, n5.getEdges().Count);
            Assert.Equal(new Edge(n2, 2.94), n5.getEdges()[0]); 
            Assert.Equal(new Edge(n4, 2), n5.getEdges()[1]);
            Assert.Equal(new Edge(n6, 2), n5.getEdges()[2]);

            /* -------------------------------- // Node 6 ------------------------------- */

            Assert.Equal("3", n6.Line);
            Assert.Equal(station2, n6.parent);
            Assert.Equal(3, n6.getEdges().Count);
            Assert.Equal(new Edge(n3, 2.94), n6.getEdges()[0]); 
            Assert.Equal(new Edge(n4, 2), n6.getEdges()[1]);
            Assert.Equal(new Edge(n5, 2), n6.getEdges()[2]);

        }

        [Fact]
        public void GRAPH_TWO_STATIONS_MULTIPLE_LINES_DIFFERENT_TEST()
        {
            /* ------------------------------- // Loading ------------------------------- */

            Commands graph = new Commands(fileName: "../../../testStations/twoStationsMultipleLinesDifferent.xml");

            Assert.Equal(graph.getStationById(522), graph.getStationByName("Test St")); // tests that the file loaded in correctly.
            Assert.Equal(graph.getStationById(523), graph.getStationByName("Fake Road")); // tests that the file loaded in correctly.

            Station? station1 = graph.getStationById(522);
            Assert.NotNull(station1);

            if (null == station1) {
                return;
            }
            Station? station2 = graph.getStationById(523);
            Assert.NotNull(station2);
            if (null == station2) {
                return;
            }

            /* ----------------------- // Testing sub nodes exist ----------------------- */

            Assert.Equal(3, station1.getSubNodes().Count); // tests that the station has 3 nodes
            Assert.Equal(2, station2.getSubNodes().Count); // tests that the station has 3 nodes

            Node n1 = station1.getSubNodes()[0];
            Node n2 = station1.getSubNodes()[1];
            Node n3 = station1.getSubNodes()[2];

            Node n4 = station2.getSubNodes()[0];
            Node n5 = station2.getSubNodes()[1];

            /* -------------------------------- // Node 1 ------------------------------- */

            Assert.Equal("1", n1.Line);
            Assert.Equal(station1, n1.parent);
            Assert.Equal(3, n1.getEdges().Count);
            Assert.Equal(new Edge(n4, 2.94), n1.getEdges()[0]); 
            Assert.Equal(new Edge(n2, 2), n1.getEdges()[1]);
            Assert.Equal(new Edge(n3, 2), n1.getEdges()[2]);

            /* -------------------------------- // Node 2 ------------------------------- */

            Assert.Equal("2", n2.Line);
            Assert.Equal(station1, n2.parent);
            Assert.Equal(3, n2.getEdges().Count);
            Assert.Equal(new Edge(n5, 2.94), n2.getEdges()[0]); 
            Assert.Equal(new Edge(n1, 2), n2.getEdges()[1]);
            Assert.Equal(new Edge(n3, 2), n2.getEdges()[2]);

            /* -------------------------------- // Node 3 ------------------------------- */

            Assert.Equal("3", n3.Line);
            Assert.Equal(station1, n3.parent);
            Assert.Equal(2, n3.getEdges().Count);
            Assert.Equal(new Edge(n1, 2), n3.getEdges()[0]);
            Assert.Equal(new Edge(n2, 2), n3.getEdges()[1]);

            /* -------------------------------- // Node 4 ------------------------------- */

            Assert.Equal("1", n4.Line);
            Assert.Equal(station2, n4.parent);
            Assert.Equal(2, n4.getEdges().Count);
            Assert.Equal(new Edge(n1, 2.94), n4.getEdges()[0]); 
            Assert.Equal(new Edge(n5, 2), n4.getEdges()[1]);

            /* -------------------------------- // Node 5 ------------------------------- */

            Assert.Equal("2", n5.Line);
            Assert.Equal(station2, n5.parent);
            Assert.Equal(2, n5.getEdges().Count);
            Assert.Equal(new Edge(n2, 2.94), n5.getEdges()[0]); 
            Assert.Equal(new Edge(n4, 2), n5.getEdges()[1]);
        }

        [Fact]
        public void GRAPH_REAL_WORLD_TEST()
        {
            /* -------------------------------------------------------------------------- */
            /*               // Tests against the defualt stations.xml file               */
            /* -------------------------------------------------------------------------- */

            /* ------------------------------- // Loading ------------------------------- */

            Commands graph = new Commands("../../../../Server/Stations.xml"); // relative path to the xml file

            /* --------------------------- // Select test data -------------------------- */

            Assert.Equal(462, graph.StationCount());

            Station? StWashingtonHts = graph.getStationById(148);

            if (null == StWashingtonHts) {
                return;
            }

            Assert.Equal("168 St-Washington Hts", StWashingtonHts.name);
            Assert.Equal(148, StWashingtonHts.id);

            
            Assert.Equal(3, StWashingtonHts.getSubNodes().Count);

            Assert.Equal("A", StWashingtonHts.getSubNodes()[0].Line);

            Assert.Equal(4, StWashingtonHts.getSubNodes()[0].getEdges().Count);

            Edge otherStationsEdge = StWashingtonHts.getSubNodes()[0].getEdges()[1];

            // Connection on the A line is to station with ID 147.
            Assert.Equal(151, otherStationsEdge.to.parent.id);
            Assert.Equal(otherStationsEdge, StWashingtonHts.getSubNodes()[0].getEdges()[1]);
            Assert.Equal(2, StWashingtonHts.getSubNodes()[0].getEdges()[2].weight);
            Assert.Equal(147, StWashingtonHts.getSubNodes()[0].getEdges()[0].to.parent.id);
            Assert.Equal(151, StWashingtonHts.getSubNodes()[0].getEdges()[1].to.parent.id);
            Assert.Equal(2, StWashingtonHts.getSubNodes()[0].getEdges()[2].weight);

        }
    }
}
