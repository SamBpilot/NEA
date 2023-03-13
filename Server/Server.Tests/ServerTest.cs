// NOTE: This file is NOT to be distributed with the server. It is only for testing purposes.
namespace Server.Tests;

// extends server class to allow us to test protected methods
public class DummyServer : NEA.Server
{
    public DummyServer() {}

    public string testEncoding(string[] input)
    {
        return encode(input);
    }

    public string[] testDecoding(string input)
    {
        return decode(input)!;
    }
}

[Trait("Server", "Server Tests")]
public class ServerTest
{
    private DummyServer _dummyServer = new DummyServer();

    [Fact]
    public void SERVER_ENCODE_TEST()
    {
        // testing a couple of parameters
        Assert.True(_dummyServer.testEncoding(new string[] {"test", "test2"})=="<test{}test2>");
        Assert.True(_dummyServer.testEncoding(new string[] {"test", "test2", "test3"})=="<test{}test2{}test3>"); // this is likely uneeded, but it's here just in case
        Assert.True(_dummyServer.testEncoding(new string[] {"test",})=="<test>");

        
        // testing a couple of potential routes
        Assert.True(_dummyServer.testEncoding(new string[] { "[{\"getOn\":\"Richmond Valley\",\"getOff\":\"Great Kills\",\"stops\":5,\"train\":\"SIR\"}]" })=="<[{\"getOn\":\"Richmond Valley\",\"getOff\":\"Great Kills\",\"stops\":5,\"train\":\"SIR\"}]>");
        Assert.True(_dummyServer.testEncoding(new string[] { "[{\"getOn\":\"Richmond Valley\",\"getOff\":\"Great Kills\",\"stops\":5,\"train\":\"SIR\"},{\"getOn\":\"Richmond Valley\",\"getOff\":\"Great Kills\",\"stops\":5,\"train\":\"SIR\"}]" })=="<[{\"getOn\":\"Richmond Valley\",\"getOff\":\"Great Kills\",\"stops\":5,\"train\":\"SIR\"},{\"getOn\":\"Richmond Valley\",\"getOff\":\"Great Kills\",\"stops\":5,\"train\":\"SIR\"}]>");

    }

    [Fact]
    public void SERVER_DECODE_TEST()
    {

        //NOTE: There is no need to test any malformed responses as these will be caught by the regex before they are decoded.

        // Testing a couple of potential commands
        Assert.Null(_dummyServer.testDecoding("<bind>"));
        Assert.True(_dummyServer.testDecoding("<IVS:{}Great Kills>")[0] == "Great Kills");
        Assert.True(_dummyServer.testDecoding("<GR:{}Richmond Valley{}Great Kills>")[1] == "Great Kills");
        Assert.True(_dummyServer.testDecoding("<GR:{}Richmond Valley{}Great Kills>")[0] == "Richmond Valley");
        Assert.True(_dummyServer.testDecoding("<Key:{}asaij;fdhluasef>")[0] == "asaij;fdhluasef");
        
    }
}
