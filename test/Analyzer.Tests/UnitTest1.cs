namespace Analyzer.Tests;

public class UnitTest1
{
    [Fact]
    public void GetsLastTokens()
    {
        const string source = "Microsoft.AspNetCore.Mvc.ControllerBase";
        var input = source.AsSpan();

        var typeName = TextHelpers.GetLastToken(ref input, '.');
        Assert.Equal("ControllerBase", new string(typeName));

        var mvc = TextHelpers.GetLastToken(ref input, '.');
        Assert.Equal("Mvc", new string(mvc));
        var aspNetCore = TextHelpers.GetLastToken(ref input, '.');
        Assert.Equal("AspNetCore", new string(aspNetCore));
        var microsoft = TextHelpers.GetLastToken(ref input, '.');
        Assert.Equal("Microsoft", new string(microsoft));
        
        Assert.Equal(0, input.Length);
    }
}