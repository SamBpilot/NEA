// NOTE: This file is NOT to be distributed with the server. It is only for testing purposes.
namespace Server.Tests;
using System.Collections.Generic;
using System.IO;

[Trait("Server", "Server Tests")]
public class CommandsTest
{
    private Commands _commands = new Commands("../../../../Server/Stations.xml"); // relative path to the xml file
    private NEA.Server _server = new NEA.Server();

    private String _hashableString1 = "55600:*(&#*$7*(&#$*(&8876867867867";
    private String _hashableString2 = "556340:*(&#45436867867867";
    private String _hashableString3 = "55630:dfw45987DJHDF*(767";

    [Fact]
    public void COMMANDS_GET_HASH_CODE()
    {
        string firstTime = HashGenerator.getStringsHash(_hashableString1).ToString();
        Assert.NotNull(firstTime);
        Assert.NotEmpty(firstTime);
        Assert.NotEqual("0", firstTime);

        string secondTime = HashGenerator.getStringsHash(_hashableString2).ToString();
        Assert.NotNull(secondTime);
        Assert.NotEmpty(secondTime);
        Assert.NotEqual("0", secondTime);
        Assert.NotEqual(firstTime, secondTime);

        string thirdTime = HashGenerator.getStringsHash(_hashableString3).ToString();
        Assert.NotNull(thirdTime);
        Assert.NotEmpty(thirdTime);
        Assert.NotEqual("0", thirdTime);
        Assert.NotEqual(firstTime, thirdTime);
        Assert.NotEqual(secondTime, thirdTime);

        // Does the hash function return the same value every time you call it
        // Because some languages do not gauruntee that (like Python) and it will cause us all sorts of problems!
        // Make sure they return the identical result even after you've done OTHER hash codes.
        Assert.Equal(firstTime, HashGenerator.getStringsHash(_hashableString1).ToString());
        Assert.Equal(secondTime, HashGenerator.getStringsHash(_hashableString2).ToString());
        Assert.Equal(thirdTime, HashGenerator.getStringsHash(_hashableString3).ToString());
    }

    [Fact]
    public void COMMANDS_CHECK_STATIONS_ARE_VALID()
    {
        Assert.True(_commands.checkStationsAreValid("Great Kills"));
        Assert.True(_commands.checkStationsAreValid("Grand Central-42 St"));
        Assert.True(_commands.checkStationsAreValid("Richmond Valley"));
        Assert.True(_commands.checkStationsAreValid("183 St"));
        Assert.True(_commands.checkStationsAreValid("Canal St"));
        Assert.True(_commands.checkStationsAreValid("Richmond Valley"));
        Assert.False(_commands.checkStationsAreValid("001 St"));
        Assert.False(_commands.checkStationsAreValid(""));
        Assert.False(_commands.checkStationsAreValid(" "));
    }

    [Fact]
    public void COMMANDS_GET_STATION_BY_ID()
    {
        Assert.True(_commands.getStationById(1)?.name == "Astoria-Ditmars Blvd");
        Assert.True(_commands.getStationById(523)?.name == "Arthur Kill");
        Assert.Null(_commands.getStationById(415)); // doesn't exist

        Assert.True(_commands.getStationById(73)?.latitude == 40.624842m);
        Assert.True(_commands.getStationById(231)?.name == "Grand St");
        Assert.True(_commands.getStationById(091)?.id == 91);

        Assert.True(_commands.getStationById(25)?.longitude == -73.985942m);
        Assert.True(_commands.getStationById(297)?.longitude == -73.915279m);
        Assert.Null(_commands.getStationById(-1));
    }

    [Fact]
    public void COMMANDS_GET_STATION_BY_NAME()
    {
        Assert.True(_commands.getStationByName("Astoria-Ditmars Blvd")?.id == 1);
        Assert.True(_commands.getStationByName("Arthur Kill")?.id == 523);
        Assert.Null(_commands.getStationByName("Oxford Circus")); // doesn't exist

        Assert.True(_commands.getStationByName("New Utrecht Av")?.latitude== 40.624842m);
        Assert.Null(_commands.getStationByName("Grand St"));
        Assert.True(_commands.getStationByName("Grand Central-42 St")?.id == 469);

        Assert.Null(_commands.getStationByName("Canal St"));
        Assert.True(_commands.getStationByName("Jay St-MetroTech")?.longitude== -73.985942m);
        Assert.True(_commands.getStationByName("215 St")?.longitude == -73.915279m);

        Assert.Null(_commands.getStationByName("Fake Name Road"));
        Assert.Null(_commands.getStationByName("Fordham Rd"));
        Assert.Null(_commands.getStationByName(" "));
    }

    [Fact]
    public void COMMANDS_GET_STATIONS_BEGGINING_WITH()
    {
        Assert.True(_commands.getStationsStartingWith("A").Count == 5);
        Assert.True(_commands.getStationsStartingWith("Grand St").Count == 2);
        Assert.True(_commands.getStationsStartingWith("Grand").Count == 5);
        Assert.True(_commands.getStationsStartingWith("Grand Central").Count == 1);
        Assert.True(_commands.getStationsStartingWith("Grand Central-42 St").Count == 1);
        Assert.True(_commands.getStationsStartingWith("3").Count == 3); // Stations that start with like 36 aren't included becuase they begin with 36 and not 3
        Assert.True(_commands.getStationsStartingWith("36").Count == 3);
        Assert.True(_commands.getStationsStartingWith("155").Count == 2);

        Assert.True(_commands.getStationsStartingWith("Oxford Circus").Count == 0);
        Assert.True(_commands.getStationsStartingWith(" ").Count == 0);
        Assert.True(_commands.getStationsStartingWith("").Count == 0);

    }

    [Fact]
    public void COMMANDS_GET_ROUTE()
    {
        List<string> routes = new List<string>(File.ReadAllLines("../../../possibleJsonRoutes.txt"));
        // Add blank line so indexes match.
        routes.Insert(0, "");


        /* -------------------------------------------------------------------------- */
        /*   Cases where A and B are on the same line and no changes are required.    */
        /* -------------------------------------------------------------------------- */

        /* --------- // Route from A to B where they are next to each other. -------- */
        // [SIR]
        Assert.Equal(routes[1], _commands.RouteMe(_commands.getStationById(522), _commands.getStationById(523))?.ToString()); // Tottenville to Arthur Kill
        Assert.Equal(routes[2], _commands.RouteMe(_commands.getStationById(516), _commands.getStationById(515))?.ToString()); // Hugenot to Annadale
        Assert.Equal(routes[3], _commands.RouteMe(_commands.getStationById(502), _commands.getStationById(501))?.ToString()); // Tompkonsville to St. George
        // [Q]
        Assert.Equal(routes[4], _commands.RouteMe(_commands.getStationById(48), _commands.getStationById(49))?.ToString()); // Avenue H to Avenue J
        Assert.Equal(routes[5], _commands.RouteMe(_commands.getStationById(50), _commands.getStationById(51))?.ToString()); // Avenue M to Kings Hwy
        Assert.Equal(routes[6], _commands.RouteMe(_commands.getStationById(51), _commands.getStationById(52))?.ToString()); // Kings Hwy to Avenue U
        // [R]
        Assert.Equal(routes[7],_commands.RouteMe(_commands.getStationById(32), _commands.getStationById(31))?.ToString()); // 36 St to 25 St
        Assert.Equal(routes[8],_commands.RouteMe(_commands.getStationById(35), _commands.getStationById(36))?.ToString()); // 59 St to Bay Ridge Av
        Assert.Equal(routes[9],_commands.RouteMe(_commands.getStationById(36), _commands.getStationById(35))?.ToString()); // Bay Ridge Av to 59 St
        //[A]
        Assert.Equal(routes[10], _commands.RouteMe(_commands.getStationById(192), _commands.getStationById(197))?.ToString()); // Rockaway Blvd to Aqueduct North Conduit Av
        Assert.Equal(routes[11], _commands.RouteMe(_commands.getStationById(197), _commands.getStationById(196))?.ToString()); // Aqueduct North Conduit Av to Aqueduct Racetrack
        Assert.Equal(routes[12], _commands.RouteMe(_commands.getStationById(196), _commands.getStationById(192))?.ToString()); // Aqueduct Racetrack to Rockaway Blvd
        // [F]/[G]
        string? route1 = _commands.RouteMe(_commands.getStationById(243), _commands.getStationById(242))?.ToString();
        Assert.True(route1 == routes[13] || route1 == routes[14]); // Church Av to Fort Hamilton Pkwy


        /* ------- // Route from A to B where there is 1 station between them. ------ */
        // [SIR]
        Assert.Equal(routes[16],_commands.RouteMe(_commands.getStationById(522), _commands.getStationById(519))?.ToString()); // Tottenville to Richmond Valley
        Assert.Equal(routes[17],_commands.RouteMe(_commands.getStationById(516), _commands.getStationById(514))?.ToString()); // Hugenot to Eltingville
        Assert.Equal(routes[18],_commands.RouteMe(_commands.getStationById(503), _commands.getStationById(501))?.ToString()); // Stapleton to St. George
        // [Q]
        Assert.Equal(routes[19],_commands.RouteMe(_commands.getStationById(48), _commands.getStationById(46))?.ToString()); // Avenue H to Cortelyou Rd
        Assert.Equal(routes[20],_commands.RouteMe(_commands.getStationById(48), _commands.getStationById(50))?.ToString()); // Avenue H to Avenue M
        Assert.Equal(routes[21],_commands.RouteMe(_commands.getStationById(56), _commands.getStationById(58))?.ToString()); // Ocean Parkway to Coney Island-Stillwell Av
        // [R]
        Assert.Equal(routes[22],_commands.RouteMe(_commands.getStationById(32), _commands.getStationById(30))?.ToString()); // 36 St to Prospect Av
        Assert.Equal(routes[23],_commands.RouteMe(_commands.getStationById(35), _commands.getStationById(37))?.ToString()); // 59 St to 77 St
        Assert.Equal(routes[24],_commands.RouteMe(_commands.getStationById(37), _commands.getStationById(35))?.ToString()); // 77 St to 59 St
        // [A]
        Assert.Equal(routes[25], _commands.RouteMe(_commands.getStationById(192), _commands.getStationById(198))?.ToString()); // Rockaway Blvd to Howard Beach JFK Airport
        Assert.Equal(routes[26], _commands.RouteMe(_commands.getStationById(197), _commands.getStationById(192))?.ToString()); // Aqueduct North Conduit Av to Rockaway Blvd
        Assert.Equal(routes[27], _commands.RouteMe(_commands.getStationById(196), _commands.getStationById(191))?.ToString()); // Aqueduct Racetrack to 88 St
        // [F]/[G]
        string route2 = _commands.RouteMe(_commands.getStationById(243), _commands.getStationById(241)) ?? "".ToString();
        Assert.True(route2 == routes[28] || route2 == routes[29]); // Church Av to 15 St Prospect Park


        /* ------ // Route from A to B where there are 2 stations between them. ----- */
        // [SIR]
        Assert.Equal(routes[31], _commands.RouteMe(_commands.getStationById(522), _commands.getStationById(518))?.ToString()); // Tottenville to Pleasant Plains
        Assert.Equal(routes[32], _commands.RouteMe(_commands.getStationById(516), _commands.getStationById(513))?.ToString()); // Hugenot to Great Kills
        Assert.Equal(routes[33], _commands.RouteMe(_commands.getStationById(504), _commands.getStationById(501))?.ToString()); // Clifton to St. George
        // [Q]
        Assert.Equal(routes[34], _commands.RouteMe(_commands.getStationById(48), _commands.getStationById(51))?.ToString()); // Avenue H to Kings Hwy
        Assert.Equal(routes[35], _commands.RouteMe(_commands.getStationById(51), _commands.getStationById(48))?.ToString()); // Kings Hwy to Avenue H
        Assert.Equal(routes[36], _commands.RouteMe(_commands.getStationById(55), _commands.getStationById(58))?.ToString()); // Brighton Beach to Coney Island-Stillwell Av
        // [R]
        Assert.Equal(routes[37], _commands.RouteMe(_commands.getStationById(32), _commands.getStationById(239))?.ToString()); // 36 St to 4 Av-9 St
        Assert.Equal(routes[38], _commands.RouteMe(_commands.getStationById(28), _commands.getStationById(31))?.ToString()); // Union St to 25 St
        Assert.Equal(routes[39], _commands.RouteMe(_commands.getStationById(34), _commands.getStationById(37))?.ToString()); // 53 St to 77 St
        // [A]
        Assert.Equal(routes[40], _commands.RouteMe(_commands.getStationById(198), _commands.getStationById(192))?.ToString()); // Howard Beach JFK Airport to Rockaway Blvd
        Assert.Equal(routes[41], _commands.RouteMe(_commands.getStationById(192), _commands.getStationById(195))?.ToString()); // Rockaway Blvd to Ozone Park Lefferts Blvd
        Assert.Equal(routes[42], _commands.RouteMe(_commands.getStationById(191), _commands.getStationById(199))?.ToString()); // 88 St to Broad Channel
        // [F]/[G]
        string route3 = _commands.RouteMe(_commands.getStationById(243), _commands.getStationById(240)) ?? "".ToString();
        Assert.True(route3 == routes[43] || route3 == routes[44]); // Church Av to 7 Av


        /*  Route from A to B where there are X stations between them where X is big  */
        /*         enough to go past a station with potential for changing lines.     */
        // [Q]
        Assert.Equal(routes[46],_commands.RouteMe(_commands.getStationById(50), _commands.getStationById(52))?.ToString()); // Avenue M to Avenue U
        Assert.Equal(routes[47], _commands.RouteMe(_commands.getStationById(53), _commands.getStationById(56))?.ToString()); // Neck Rd to Ocean Pkwy
        Assert.Equal(routes[48], _commands.RouteMe(_commands.getStationById(45), _commands.getStationById(49))?.ToString()); // Beverley Rd to Avenue J
        // [R]
        Assert.Equal(routes[49], _commands.RouteMe(_commands.getStationById(31), _commands.getStationById(33))?.ToString()); // 25 St to 45 St
        Assert.Equal(routes[50], _commands.RouteMe(_commands.getStationById(33), _commands.getStationById(31))?.ToString()); // 45 St to 25 St
        Assert.Equal(routes[51], _commands.RouteMe(_commands.getStationById(34), _commands.getStationById(31))?.ToString()); // 53 St to 25 St

        /* ---------------------- Travel one change one required tests. ---------------------- */

        Assert.Equal(routes[52],_commands.RouteMe(_commands.getStationById(36), _commands.getStationById(71))?.ToString()); // Bay Ridge Av to 8 Av
        Assert.Equal(routes[53],_commands.RouteMe(_commands.getStationById(34), _commands.getStationById(71))?.ToString()); // 53 St to 8 Av
        Assert.Equal(routes[54],_commands.RouteMe(_commands.getStationById(273), _commands.getStationById(461))?.ToString()); // Queens Plaza to Queensboro Plaza
        Assert.Equal(routes[55],_commands.RouteMe(_commands.getStationById(43), _commands.getStationById(142))?.ToString()); // Parkside Av to Botanic Garden
        Assert.Equal(routes[56],_commands.RouteMe(_commands.getStationById(343), _commands.getStationById(141))?.ToString()); // Nostrand Av to Park Pl
        Assert.Equal(routes[57],_commands.RouteMe(_commands.getStationById(177), _commands.getStationById(141))?.ToString()); // Clinton-Washington Avs to Park Pl
        Assert.Equal(routes[58],_commands.RouteMe(_commands.getStationById(141), _commands.getStationById(177))?.ToString()); // Park Pl to Clinton-Washington Avs

        /* ---------------------- Travel many change one required tests. ---------------------- */

        Assert.Equal(routes[59],_commands.RouteMe(_commands.getStationById(71), _commands.getStationById(59))?.ToString()); // 8 Av to 9 Av
        Assert.Equal(routes[60],_commands.RouteMe(_commands.getStationById(36), _commands.getStationById(59))?.ToString()); // Bay Ridge Av to 9 Av
        Assert.Equal(routes[61],_commands.RouteMe(_commands.getStationById(53), _commands.getStationById(253))?.ToString()); // Neck Rd to Neptune Av
        Assert.Equal(routes[62],_commands.RouteMe(_commands.getStationById(75), _commands.getStationById(36))?.ToString()); // 20 Av to Bay Ridge Av
        Assert.Equal(routes[63],_commands.RouteMe(_commands.getStationById(75), _commands.getStationById(70))?.ToString()); // 20 Av to Bay 50 St

        /* ---------------------- Travel one change many required tests. ---------------------- */

        Assert.Equal(routes[64],_commands.RouteMe(_commands.getStationById(353), _commands.getStationById(344))?.ToString()); // President Street-Medgar Evers College to Kingston Av
        Assert.Equal(routes[65],_commands.RouteMe(_commands.getStationById(59), _commands.getStationById(30))?.ToString()); // 9 Av to Prospect Av
        Assert.Equal(routes[66],_commands.RouteMe(_commands.getStationById(59), _commands.getStationById(71))?.ToString()); // 9 Av to 8 Av

        /* ---------------------- 2 Changes required tests. ---------------------- */

        Assert.Equal(routes[67],_commands.RouteMe(_commands.getStationById(70), _commands.getStationById(47))?.ToString()); // Bay 50 St to Newkirk Plaza
        Assert.Equal(routes[68],_commands.RouteMe(_commands.getStationById(253), _commands.getStationById(47))?.ToString()); // Neptune Av to Newkirk Plaza
        Assert.Equal(routes[69],_commands.RouteMe(_commands.getStationById(30), _commands.getStationById(57))?.ToString()); // Prospect Av to W 8 St-NY Aquarium

        /* ---------------------- Express required tests. ---------------------- */

        Assert.Equal(routes[70],_commands.RouteMe(_commands.getStationById(346), _commands.getStationById(342))?.ToString()); // Sutter Av-Rutland Rd to Franklin Avenue-Medgar Evers College
        Assert.Equal(routes[71],_commands.RouteMe(_commands.getStationById(342), _commands.getStationById(346))?.ToString()); // Franklin Avenue-Medgar Evers College to Sutter Av-Rutland Rd
        Assert.Equal(routes[72],_commands.RouteMe(_commands.getStationById(359), _commands.getStationById(336))?.ToString()); // Flatbush Av-Brooklyn College to Hoyt St
        Assert.Equal(routes[73],_commands.RouteMe(_commands.getStationById(36), _commands.getStationById(32))?.ToString()); // Bay Ridge Av to 36 St
        Assert.Equal(routes[74],_commands.RouteMe(_commands.getStationById(50), _commands.getStationById(54))?.ToString()); // Avenue M to Sheepshead Bay
        Assert.Equal(routes[75],_commands.RouteMe(_commands.getStationById(45), _commands.getStationById(51))?.ToString()); // Beverley Rd to Kings Hwy
        Assert.Equal(routes[76],_commands.RouteMe(_commands.getStationById(56), _commands.getStationById(51))?.ToString()); // Ocean Pkwy to Kings Hwy
        Assert.Equal(routes[77],_commands.RouteMe(_commands.getStationById(139), _commands.getStationById(181))?.ToString()); // Franklin Av to Utica Av
        Assert.Equal(routes[78],_commands.RouteMe(_commands.getStationById(180), _commands.getStationById(132))?.ToString()); // Kingston-Throop Avs to Broadway Junction
        Assert.Equal(routes[79],_commands.RouteMe(_commands.getStationById(192), _commands.getStationById(132))?.ToString()); // Rockaway Blvd to Broadway Junction
        Assert.Equal(routes[80],_commands.RouteMe(_commands.getStationById(51), _commands.getStationById(53))?.ToString()); // Kings Hwy to Neck Rd
        Assert.Equal(routes[81],_commands.RouteMe(_commands.getStationById(50), _commands.getStationById(53))?.ToString()); // Avenue M to Neck Rd

        /* ---------------------- Random Long Routes required tests. ---------------------- */

        Assert.Equal(routes[82],_commands.RouteMe(_commands.getStationById(206), _commands.getStationById(108))?.ToString()); // Beach 44 St to Middle Village-Metropolitan Av
        Assert.Equal(routes[83],_commands.RouteMe(_commands.getStationById(303), _commands.getStationById(289))?.ToString()); // 157 St to Bedford-Nostrand Avs
        Assert.Equal(routes[84],_commands.RouteMe(_commands.getStationById(449), _commands.getStationById(199))?.ToString()); // 111 St to Broad Channel
        Assert.Equal(routes[85],_commands.RouteMe(_commands.getStationById(469), _commands.getStationById(198))?.ToString()); // Grand Central-42 St to Howard Beach-JFK Airport
        Assert.Equal(routes[86],_commands.RouteMe(_commands.getStationById(198), _commands.getStationById(1))?.ToString()); // Howard Beach-JFK Airport to Astoria-Ditmars Blvd
        Assert.Equal(routes[87],_commands.RouteMe(_commands.getStationById(263), _commands.getStationById(437))?.ToString()); // 63 Dr-Rego Park to 145 St
        Assert.Equal(routes[88],_commands.RouteMe(_commands.getStationById(278), _commands.getStationById(117))?.ToString()); // Jamaica Center-Parsons/Archer to 14 St-Union Sq.
        Assert.Equal(routes[89],_commands.RouteMe(_commands.getStationById(257), _commands.getStationById(197))?.ToString()); // Sutphin Blvd to Aqueduct-N Conduit Av
        Assert.Equal(routes[90],_commands.RouteMe(_commands.getStationById(197), _commands.getStationById(257))?.ToString()); // Aqueduct-N Conduit Av to Sutphin Blvd

        /* -------------------------------------------------------------------------- */
        /*                      // Expected negative test cases.                      */
        /* -------------------------------------------------------------------------- */


        /* -- // Route from A to B where one station is on the SIR and one is not. -- */
        /* --- // Expect to not be able to route as these lines are not connected. -- */
        Assert.Null(_commands.RouteMe(_commands.getStationById(522), _commands.getStationById(199))); // Tottenville to Broad Channel
        Assert.Null(_commands.RouteMe(_commands.getStationById(199), _commands.getStationById(522))); // Broad Channel to Tottenville
    }
}
