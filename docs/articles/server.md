# Installing The Server

> [!TIP]
> You should install git.

## Installing the server

### Stable

To install the server, navigate to the [releases page](https://github.com/wotanut/NEA/releases) and download the latest release marked `server`. Extract the contents of the zip file to a folder of your choice.

### Dev

To install development builds, you __must__ have git installed. To clone the repository run:

``` git clone https://github.com/wotanut/NEA.git ```

Then navigate to the `NEA` folder and run the server.

## Running the server

To run the server, navigate to the folder you extracted and run


``` dotnet run ```

## Command Line Options

There are a few command line options you can use to configure the server.

### [LogLevel](#tab/loglevel)

``` dotnet run --loglevel=<loglevel> ```


The log level to use. The default is level 5.

Log levels are as follows:

| Level | Description |
|---------|--------|
| Level 1 | Errors |
| Level 2 | Level 1 + Warnings |
| Level 3 | Level 2 + Success |
| Level 4 | Level 3 + Information |
| Level 5 | Level 4 + Debug |

### [file](#tab/file)

``` dotnet run --file=<full path to file>```

The stations file to use for the server. The defualt is the staions.xml file bundled with the server. Please note the file must be in the correct format. Below is an example of the format.

```
<row>
    <Stop_Name>Van Cortlandt Park-242 St</Stop_Name>
    <Station_ID>293</Station_ID>
    <Daytime_Routes>1</Daytime_Routes>
    <GTFS_Latitude>40.889248</GTFS_Latitude>
    <GTFS_Longitude>-73.898583</GTFS_Longitude>
    <Surrounding_Stations>294</Surrounding_Stations>
 </row>	
 ```

 > [!TIP]
 > Want a surrounding station to only be added on a certain same line? Simply add that line in brackets after the station id. For example, `294[piccadilly]` would only draw a connection to station 294 on the piccadilly line.